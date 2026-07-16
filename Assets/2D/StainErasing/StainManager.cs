using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StainManager : MonoBehaviour
{
    [Header("순서대로 배치할 얼룩들")]
    public StainEraser[] stains;
    private int currentIndex = 0;

    [Header("커스텀 커서 설정")]
    public Sprite cursorSprite;
    public float cursorSizeMultiplier = 2f;
    private Image cursorUI;

    [Header("최종 완료 이벤트")]
    public UnityEvent onAllCleared;

    [Header("사운드")]
    public AudioClip clearSound;
    private AudioSource audioSource;

    // ★ 추가: 청소(뽀드득) 효과음 관련 변수
    [Header("청소 효과음 (마우스 클릭 유지)")]
    public AudioClip wipeSound;
    private AudioSource wipeAudioSource; // 청소 소리만 독립적으로 제어할 오디오 소스

    void Start()
    {
        // 커서 설정
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 클리어 사운드용 오디오 소스
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // ★ 추가: 청소 사운드용 오디오 소스 동적 생성 및 설정
        wipeAudioSource = gameObject.AddComponent<AudioSource>();
        wipeAudioSource.clip = wipeSound;
        wipeAudioSource.loop = true; // 마우스를 계속 누르고 있으면 반복 재생 (원치 않으면 false)
        wipeAudioSource.playOnAwake = false;

        Time.timeScale = 1f;
        if (stains.Length > 0)
        {
            ActivateNextStain();
        }

        CreateCursor();
    }

    void CreateCursor()
    {
        if (cursorSprite == null) return;
        GameObject go = new GameObject("GlobalCursor");
        Canvas canvas = FindFirstObjectByType<Canvas>();

        // 캔버스가 없으면 에러 방지
        if (canvas == null) return;

        go.transform.SetParent(canvas.transform, false);
        cursorUI = go.AddComponent<Image>();
        cursorUI.sprite = cursorSprite;
        cursorUI.raycastTarget = false;
        cursorUI.rectTransform.sizeDelta = new Vector2(stains[0].brushSize * cursorSizeMultiplier, stains[0].brushSize * cursorSizeMultiplier);

        // 커스텀 커서를 쓰므로 시스템 커서는 숨김
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. 커서 위치 업데이트
        if (cursorUI != null)
        {
            Vector2 localPoint;
            if (cursorUI.canvas != null)
            {
                RectTransform canvasRect = cursorUI.canvas.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cursorUI.canvas.worldCamera, out localPoint);
                cursorUI.rectTransform.localPosition = localPoint;
            }
        }

        // ★ 2. 마우스 클릭 상태에 따른 사운드 제어 (모든 청소가 끝나기 전까지만 작동)
        if (currentIndex < stains.Length)
        {
            // 마우스를 눌렀을 때 (재생 중이 아닐 때만 재생하여 돌림노래 방지)
            if (Input.GetMouseButtonDown(0))
            {
                if (wipeSound != null && !wipeAudioSource.isPlaying)
                {
                    wipeAudioSource.Play();
                }
            }
            // 마우스에서 손을 뗐을 때 (즉시 중지)
            else if (Input.GetMouseButtonUp(0))
            {
                if (wipeAudioSource.isPlaying)
                {
                    wipeAudioSource.Stop();
                }
            }
        }
    }

    public void OnStainCleared()
    {
        currentIndex++;
        if (currentIndex < stains.Length)
        {
            ActivateNextStain();
        }
        else
        {
            Debug.Log("모든 얼룩 클리어!");

            // ★ 추가: 청소 완료 시 재생 중이던 뽀드득 사운드 즉시 강제 종료
            if (wipeAudioSource != null && wipeAudioSource.isPlaying)
            {
                wipeAudioSource.Stop();
            }

            // 기존 이벤트 호출 (혹시 다른 효과음 등을 연결했다면 실행됨)
            onAllCleared.Invoke();

            if (cursorUI != null) cursorUI.gameObject.SetActive(false);
            Cursor.visible = true;

            ReturnToMainGame();
        }
    }

    private void ActivateNextStain()
    {
        for (int i = 0; i < stains.Length; i++)
        {
            stains[i].isMyTurn = (i == currentIndex);
        }
        Debug.Log($"현재 지워야 할 얼룩: {currentIndex + 1}번");
    }

    private void ReturnToMainGame()
    {
        Debug.Log("미니게임 성공! 메인으로 돌아갑니다.");
        if (clearSound != null) audioSource.PlayOneShot(clearSound);

        // 1. 성공했다고 기록 (GameManager가 이걸 보고 수리 완료 처리함)
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        // 2. GameScene으로 이동
        SceneManager.LoadScene("GameScene");
    }

    // ★★★ 추가: Exit 버튼에서 호출할 함수 ★★★
    public void ExitMiniGame()
    {
        Debug.Log("청소를 취소하고 메인 게임 씬으로 돌아갑니다.");

        // 메인 씬으로 돌아가기 전에 커서를 다시 보이게 설정
        if (cursorUI != null) cursorUI.gameObject.SetActive(false);
        Cursor.visible = true;

        // 미니게임을 완료하지 않고 나가는 것이므로 성공 여부를 0(실패/취소)으로 확실히 저장
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        // 메인 게임 씬으로 복귀
        SceneManager.LoadScene("GameScene");
    }
}
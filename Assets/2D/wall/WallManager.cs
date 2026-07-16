using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WallManager : MonoBehaviour
{
    [Header("순서대로 수리할 균열들")]
    public WallEraser[] cracks;
    private int currentIndex = 0;

    [Header("커스텀 커서 설정 (시멘트 흙손 등)")]
    public Sprite cursorSprite;
    public float cursorSizeMultiplier = 2f;
    private Image cursorUI;

    [Header("최종 완료 이벤트")]
    public UnityEvent onAllRepaired;

    [Header("사운드")]
    public AudioClip clearSound;
    private AudioSource audioSource;

    // ★ 추가: 수리 효과음 관련 변수
    [Header("수리 효과음 (마우스 클릭 유지)")]
    public AudioClip repairSound;
    private AudioSource repairAudioSource; // 수리 소리만 독립적으로 제어할 오디오 소스

    void Start()
    {
        // 커서 설정
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 클리어 사운드용 오디오 소스
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // ★ 추가: 수리 사운드용 오디오 소스 동적 생성 및 설정
        repairAudioSource = gameObject.AddComponent<AudioSource>();
        repairAudioSource.clip = repairSound;
        repairAudioSource.loop = true; // 4초가 지나도 계속 누르고 있으면 반복 재생되게 (원치 않으면 false로 변경)
        repairAudioSource.playOnAwake = false;

        Time.timeScale = 1f;
        if (cracks.Length > 0)
        {
            ActivateNextCrack();
        }

        CreateCursor();
    }

    void CreateCursor()
    {
        if (cursorSprite == null) return;
        GameObject go = new GameObject("GlobalCursor");
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null) return;

        go.transform.SetParent(canvas.transform, false);
        cursorUI = go.AddComponent<Image>();
        cursorUI.sprite = cursorSprite;
        cursorUI.raycastTarget = false;

        cursorUI.rectTransform.sizeDelta = new Vector2(cracks[0].brushSize * cursorSizeMultiplier, cracks[0].brushSize * cursorSizeMultiplier);

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

        // ★ 2. 마우스 클릭 상태에 따른 사운드 제어 (모든 수리가 끝나기 전까지만 작동)
        if (currentIndex < cracks.Length)
        {
            // 마우스를 눌렀을 때 (재생 중이 아닐 때만 재생하여 돌림노래 방지)
            if (Input.GetMouseButtonDown(0))
            {
                if (repairSound != null && !repairAudioSource.isPlaying)
                {
                    repairAudioSource.Play();
                }
            }
            // 마우스에서 손을 뗐을 때 (즉시 중지)
            else if (Input.GetMouseButtonUp(0))
            {
                if (repairAudioSource.isPlaying)
                {
                    repairAudioSource.Stop();
                }
            }
        }
    }

    public void OnCrackRepaired()
    {
        currentIndex++;
        if (currentIndex < cracks.Length)
        {
            ActivateNextCrack();
        }
        else
        {
            Debug.Log("모든 균열 수리 완료!");

            // ★ 추가: 수리 완료 시 재생 중이던 수리 사운드 즉시 강제 종료
            if (repairAudioSource != null && repairAudioSource.isPlaying)
            {
                repairAudioSource.Stop();
            }

            onAllRepaired.Invoke();

            if (cursorUI != null) cursorUI.gameObject.SetActive(false);
            Cursor.visible = true;

            ReturnToMainGame();
        }
    }

    private void ActivateNextCrack()
    {
        for (int i = 0; i < cracks.Length; i++)
        {
            cracks[i].isMyTurn = (i == currentIndex);
        }
        Debug.Log($"현재 수리해야 할 균열: {currentIndex + 1}번");
    }

    private void ReturnToMainGame()
    {
        Debug.Log("미니게임 성공! 메인 게임 씬으로 돌아갑니다.");

        if (clearSound != null) audioSource.PlayOneShot(clearSound);

        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("gamescene");
    }

    // ★★★ 추가: Exit 버튼에서 호출할 함수 ★★★
    public void ExitMiniGame()
    {
        Debug.Log("수리를 취소하고 메인 게임 씬으로 돌아갑니다.");

        // 메인 씬으로 돌아가기 전에 커서를 다시 보이게 설정
        if (cursorUI != null) cursorUI.gameObject.SetActive(false);
        Cursor.visible = true;

        // 미니게임을 완료하지 않고 나가는 것이므로 성공 여부를 0(실패/취소)으로 확실히 저장
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        // 메인 게임 씬으로드
        SceneManager.LoadScene("gamescene");
    }
}
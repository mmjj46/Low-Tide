using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ★ 1. 씬 이동을 위해 추가 필수!

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

    void Start()
    {
        // 커서 설정
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

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

            // 기존 이벤트 호출 (혹시 다른 효과음 등을 연결했다면 실행됨)
            onAllCleared.Invoke();

            if (cursorUI != null) cursorUI.gameObject.SetActive(false);
            Cursor.visible = true;

            // ★★★ 2. 게임 클리어 처리 및 씬 이동 로직 추가 ★★★
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

    // ★★★ 핵심 함수: 성공 기록 후 메인으로 복귀 ★★★
    private void ReturnToMainGame()
    {
        Debug.Log("미니게임 성공! 메인으로 돌아갑니다.");

        // 1. 성공했다고 기록 (GameManager가 이걸 보고 수리 완료 처리함)
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        // 2. GameScene으로 이동
        // (주의: 씬 이름이 정확히 "GameScene"이어야 합니다. 다르면 수정해주세요!)
        SceneManager.LoadScene("GameScene");
    }
}
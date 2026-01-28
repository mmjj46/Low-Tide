using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StainManager : MonoBehaviour
{
    [Header("순서대로 배치할 얼룩들")]
    public StainEraser[] stains;
    private int currentIndex = 0;

    [Header("커스텀 커서 설정 (매니저에서 통합 관리)")]
    public Sprite cursorSprite;
    public float cursorSizeMultiplier = 2f;
    private Image cursorUI;

    [Header("최종 완료 이벤트")]
    public UnityEvent onAllCleared;

    void Start()
    {
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
        go.transform.SetParent(canvas.transform, false);
        cursorUI = go.AddComponent<Image>();
        cursorUI.sprite = cursorSprite;
        cursorUI.raycastTarget = false;
        cursorUI.rectTransform.sizeDelta = new Vector2(stains[0].brushSize * cursorSizeMultiplier, stains[0].brushSize * cursorSizeMultiplier);
        Cursor.visible = false;
    }

    void Update()
    {
        if (cursorUI != null)
        {
            Vector2 localPoint;
            RectTransform canvasRect = cursorUI.canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cursorUI.canvas.worldCamera, out localPoint);
            cursorUI.rectTransform.localPosition = localPoint;
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
            onAllCleared.Invoke();
            if (cursorUI != null) cursorUI.gameObject.SetActive(false);
            Cursor.visible = true;
        }
    }

    private void ActivateNextStain()
    {
        // 모든 얼룩의 turn을 끄고 현재 순서만 켬
        for (int i = 0; i < stains.Length; i++)
        {
            stains[i].isMyTurn = (i == currentIndex);
        }
        Debug.Log($"현재 지워야 할 얼룩: {currentIndex + 1}번");
    }
}
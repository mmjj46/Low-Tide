using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueHistoryManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject historyPanel;
    public Transform contentArea;
    public ScrollRect scrollRect;
    public GameObject templateObject; // Hierarchy의 Content 안에 있는 그 텍스트

    [Header("Speaker Styles")]
    public SpeakerStyle speakerA;
    public SpeakerStyle speakerB;

    [System.Serializable]
    public class SpeakerStyle
    {
        public string nameID;
        public Color textColor = Color.white;
        public TMP_FontAsset font;
    }

    private bool isOpen = false;

    void Awake()
    {
        // [수정] 원본 템플릿은 게임 시작하자마자 비활성화해서 숨깁니다.
        // 부모를 해제하지 않아야 Instantiate 시 UI 설정이 그대로 복사됩니다.
        if (templateObject != null)
        {
            templateObject.SetActive(false);
        }
    }

    public void ToggleHistory()
    {
        isOpen = !isOpen;
        historyPanel.SetActive(isOpen);
        if (isOpen) StartCoroutine(AutoScrollToBottom());
    }

    public void CloseHistory()
    {
        isOpen = false;
        historyPanel.SetActive(false);
    }

    public void AddLog(string rawLine)
    {
        if (templateObject == null) return;

        // 1. 템플릿 복사 (부모는 contentArea로 지정)
        GameObject newLog = Instantiate(templateObject, contentArea);

        // 2. 복사본은 활성화 (원본은 꺼져있으므로)
        newLog.SetActive(true);

        // 3. 텍스트 컴포넌트 찾기
        TMP_Text tmp = newLog.GetComponentInChildren<TMP_Text>();

        // [오류 방지] 여기서 tmp가 null이면 에러를 확실히 잡아줍니다.
        if (tmp == null)
        {
            Debug.LogError("TemplateObject 내부에 TextMeshPro 컴포넌트가 없습니다!");
            return;
        }

        // --- 데이터 적용 로직 ---
        string[] parts = rawLine.Split(new char[] { ':' }, 2);

        if (parts.Length > 1)
        {
            string speakerName = parts[0].Trim();
            string dialogue = parts[1].Trim();

            if (speakerName == speakerA.nameID) ApplyStyle(tmp, speakerA, speakerName, dialogue);
            else if (speakerName == speakerB.nameID) ApplyStyle(tmp, speakerB, speakerName, dialogue);
            else
            {
                tmp.text = rawLine;
                tmp.color = Color.white;
            }
        }
        else
        {
            // 이름 없는 경우 주인공 스타일 적용
            tmp.color = speakerA.textColor;
            if (speakerA.font != null) tmp.font = speakerA.font;
            tmp.text = rawLine;
        }

        // 레이아웃 강제 갱신 (텍스트 겹침 방지)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea.GetComponent<RectTransform>());

        if (isOpen) StartCoroutine(AutoScrollToBottom());
    }

    void ApplyStyle(TMP_Text target, SpeakerStyle style, string name, string text)
    {
        target.color = style.textColor;
        if (style.font != null) target.font = style.font;
        target.lineSpacing = 5.0f;
        target.text = $"<b>{name}</b> : {text}";
    }

    IEnumerator AutoScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
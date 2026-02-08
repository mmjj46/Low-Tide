using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueHistoryManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject historyPanel;
    public Transform contentArea;
    public ScrollRect scrollRect;
    public GameObject templateObject;

    [Header("Speaker Styles")]
    public List<SpeakerStyle> speakerProfiles = new List<SpeakerStyle>();

    public SpeakerStyle narrationStyle;

    [System.Serializable]
    public class SpeakerStyle
    {
        public string nameID; // ID (예: "???", "{userName}", "Player")
        public Color textColor = Color.white;
        public TMP_FontAsset font;
    }

    private bool isOpen = false;
    private string playerName;
    private const string PLAYER_NAME_KEY = "PlayerName";
    private Color defaultColor = Color.white; // 템플릿의 기본 색상 저장용

    void Awake()
    {
        if (templateObject == null)
        {
            Debug.LogError("[DialogueHistoryManager] Template Object가 연결되지 않았습니다! Inspector를 확인해주세요.");
        }
        else
        {
            // 템플릿의 원래 색상을 저장해둡니다. (스타일 없을 때 사용)
            var textComp = templateObject.GetComponentInChildren<TMP_Text>();
            if (textComp != null) defaultColor = textComp.color;

            templateObject.SetActive(false);
        }

        if (contentArea == null)
        {
            Debug.LogError("[DialogueHistoryManager] Content Area가 연결되지 않았습니다!");
        }
    }

    void Start()
    {
        playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "플레이어");
    }

    public void ToggleHistory()
    {
        isOpen = !isOpen;
        if (historyPanel != null)
        {
            historyPanel.SetActive(isOpen);
            if (isOpen && scrollRect != null) StartCoroutine(AutoScrollToBottom());
        }
    }

    public void CloseHistory()
    {
        isOpen = false;
        if (historyPanel != null) historyPanel.SetActive(false);
    }

    string GetProcessedText(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace("{userName}", playerName);
    }

    public void AddLog(string rawLine)
    {
        if (templateObject == null || contentArea == null) return;

        GameObject newLog = Instantiate(templateObject, contentArea);
        newLog.SetActive(true);

        TMP_Text tmp = newLog.GetComponentInChildren<TMP_Text>();

        if (tmp == null)
        {
            Debug.LogError("TemplateObject 내부에 TextMeshPro 컴포넌트가 없습니다!");
            return;
        }

        // --- 데이터 적용 로직 ---
        string[] parts = rawLine.Split(new char[] { ':' }, 2);

        if (parts.Length > 1)
        {
            string rawSpeaker = parts[0].Trim();
            string rawDialogue = parts[1].Trim();

            // 화면 표시 이름 처리
            string displaySpeaker = rawSpeaker;

            if (rawSpeaker == "{userName}")
            {
                displaySpeaker = playerName;
            }
            else
            {
                displaySpeaker = GetProcessedText(rawSpeaker);
            }

            string displayDialogue = GetProcessedText(rawDialogue);

            // 스타일 검색 로직
            SpeakerStyle foundStyle = null;
            if (speakerProfiles != null)
            {
                foundStyle = speakerProfiles.Find(x => x.nameID == rawSpeaker);

            }

            if (foundStyle != null)
            {
                ApplyStyle(tmp, foundStyle, displaySpeaker, displayDialogue);
            }
            else
            {
                // 스타일을 못 찾으면 템플릿 원래 색상 사용
                tmp.text = $"<b>{displaySpeaker}</b> : {displayDialogue}";
                tmp.color = defaultColor;
            }
        }
        else
        {
            // 지문/내레이션 처리
            string processedLine = GetProcessedText(rawLine);

            if (narrationStyle != null && !string.IsNullOrEmpty(narrationStyle.nameID))
            {
                tmp.color = narrationStyle.textColor;
                if (narrationStyle.font != null) tmp.font = narrationStyle.font;
            }
            else
            {
                // 스타일 없으면 기본 색상
                tmp.color = defaultColor;
            }

            tmp.text = processedLine;
        }

        Canvas.ForceUpdateCanvases();
        if (contentArea.GetComponent<RectTransform>() != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea.GetComponent<RectTransform>());

        if (isOpen && scrollRect != null) StartCoroutine(AutoScrollToBottom());
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
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }
}
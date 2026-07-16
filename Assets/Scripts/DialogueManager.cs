using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpeakerProfile
    {
        public string speakerName;
        public Color nameColor;
        public Color dialogueColor;
        public TMP_FontAsset font;
        public float fontSize;
    }

    private struct ChoiceData
    {
        public string targetID;
        public string statName;
    }

    [System.Serializable]
    public struct DailyDialogue
    {
        public int day;
        public TextAsset dialogueFile;
    }

    [Header("UI 컴포넌트 연결")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;

    [Header("History Settings")]
    public DialogueHistoryManager historyManager;

    [Header("배경 비디오 연결")]
    public VideoPlayer backgroundVideoPlayer;

    [Header("선택지 UI 설정")]
    public Button[] optionButtons;
    public TextMeshProUGUI[] optionButtonTexts;

    [Header("화자별 스타일 설정")]
    public List<SpeakerProfile> speakerProfiles;
    public SpeakerProfile narrationProfile;

    [Header("날짜별 대본 데이터")]
    public List<DailyDialogue> dailyDialogues;

    [Header("씬 설정")]
    public string returnSceneName = "Main3DScene";

    [Header("설정")]
    public float typingSpeed = 0.05f;

    [Header("오디오")]
    public AudioClip[] clipsShort;
    public AudioClip[] clipsMedium;
    public AudioClip[] clipsLong;

    private List<string> lines = new List<string>();
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isWaitingForChoice = false;
    private AudioSource audioSource;
    private bool isTyping = false;
    private string currentContent;
    private Coroutine typingCoroutine;
    private List<ChoiceData> currentChoices = new List<ChoiceData>();
    private string playerName;
    private string responderName;
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string RESPONDER_NAME_KEY = "ResponderName";
    private const string CURRENT_DAY_KEY = "CurrentDay";

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        audioSource = GetComponent<AudioSource>();
        playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "플레이어");
        responderName = PlayerPrefs.GetString(RESPONDER_NAME_KEY, "상대방");

        if (nameText != null) nameText.text = "";
        if (backgroundVideoPlayer != null && !backgroundVideoPlayer.isPlaying) backgroundVideoPlayer.Play();

        InitOptionButtons();
        int currentDay = PlayerPrefs.GetInt(CURRENT_DAY_KEY, 7);
        LoadDialogueForDay(currentDay);
    }

    void LoadDialogueForDay(int day)
    {
        TextAsset targetDialogue = null;
        foreach (var daily in dailyDialogues)
        {
            if (daily.day == day) { targetDialogue = daily.dialogueFile; break; }
        }

        if (targetDialogue != null)
        {
            lines.Clear();
            lines.AddRange(targetDialogue.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries));
            isDialogueActive = true;
            ShowNextLine();
        }
        else Debug.LogError($"Day {day}에 해당하는 텍스트 파일이 연결되지 않았습니다!");
    }

    void InitOptionButtons()
    {
        if (optionButtons == null) return;
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            if (optionButtons[i] != null)
            {
                optionButtons[i].gameObject.SetActive(false);
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            }
        }
    }

    void Update()
    {
        if (isWaitingForChoice || !isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentContent;
                isTyping = false;
                if (audioSource != null) audioSource.Stop();
            }
            else ShowNextLine();
        }
    }

    void ShowNextLine()
    {
        if (currentLineIndex >= lines.Count) { EndDialogue(); return; }

        string rawLine = lines[currentLineIndex].Trim();
        if (rawLine.StartsWith("[ID:") || rawLine.StartsWith("[GOTO:")) { currentLineIndex++; ShowNextLine(); return; }
        if (rawLine.Equals("[CHOICE]")) { StartChoiceMode(); return; }

        ParseAndApplyStyle(rawLine, out _, out string content);
        currentContent = content;

        if (historyManager != null) historyManager.AddLog(content);
        PlayDialogueSound(currentContent);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentContent));
        currentLineIndex++;
    }

    void StartChoiceMode()
    {
        isWaitingForChoice = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        currentChoices.Clear();
        currentLineIndex++;

        int buttonIndex = 0;
        while (currentLineIndex < lines.Count && lines[currentLineIndex].Trim().StartsWith(">"))
        {
            string[] parts = lines[currentLineIndex].Substring(1).Split(':');
            if (parts.Length >= 2 && buttonIndex < optionButtons.Length)
            {
                optionButtons[buttonIndex].gameObject.SetActive(true);
                optionButtonTexts[buttonIndex].text = GetProcessedText(parts[0].Trim());
                currentChoices.Add(new ChoiceData { targetID = parts[1].Trim(), statName = (parts.Length > 2) ? parts[2].Trim() : "" });
                buttonIndex++;
            }
            currentLineIndex++;
        }
    }

    public void OnOptionSelected(int index)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ChoiceData choice = currentChoices[index];
        if (!string.IsNullOrEmpty(choice.statName)) PlayerPrefs.SetInt(choice.statName, PlayerPrefs.GetInt(choice.statName, 0) + 1);
        foreach (var btn in optionButtons) btn.gameObject.SetActive(false);
        isWaitingForChoice = false;
        JumpToID(choice.targetID);
    }

    void JumpToID(string targetID)
    {
        string searchTag = $"[ID:{targetID}]";
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim().Equals(searchTag)) { currentLineIndex = i + 1; ShowNextLine(); return; }
        }
    }

    string GetProcessedText(string text)
    {
        return text.Replace("{userName}", playerName).Replace("{username}", playerName).Replace("{Name}", playerName).Replace("{name}", playerName).Replace("{responderName}", responderName);
    }

    void ParseAndApplyStyle(string rawLine, out string speakerName, out string content)
    {
        if (!rawLine.Contains(":"))
        {
            speakerName = ""; content = GetProcessedText(rawLine);
            if (nameText != null) nameText.text = "";
            ApplyProfile(narrationProfile);
            return;
        }

        string[] parts = rawLine.Split(new char[] { ':' }, 2);
        string rawSpeaker = parts[0].Trim();
        content = GetProcessedText(parts[1].Trim());

        ApplyStyleByName(rawSpeaker);

        string displaySpeaker = (rawSpeaker.ToLower() == "{username}" || rawSpeaker.ToLower() == "{name}") ? playerName :
                                (rawSpeaker == "{responderName}") ? responderName : GetProcessedText(rawSpeaker);

        if (nameText != null) nameText.text = displaySpeaker;
        speakerName = displaySpeaker;
    }

    void ApplyStyleByName(string name)
    {
        string lookupName = name.Trim();
        if (lookupName.ToLower() == "{username}" || lookupName.ToLower() == "{name}") lookupName = "{username}";
        else if (lookupName == "{responderName}") lookupName = "{responderName}";

        SpeakerProfile found = speakerProfiles.Find(x => x.speakerName == lookupName);

        // 프로필을 찾으면 적용, 없으면 기본 나레이션 프로필 적용하여 폰트 초기화
        if (!string.IsNullOrEmpty(found.speakerName)) ApplyProfile(found);
        else ApplyProfile(narrationProfile);
    }

    void ApplyProfile(SpeakerProfile profile)
    {
        dialogueText.color = profile.dialogueColor;

        if (profile.font != null) dialogueText.font = profile.font;
        else dialogueText.font = narrationProfile.font; // 프로필 폰트가 없으면 나레이션 기본폰트 적용

        if (profile.fontSize > 0) dialogueText.fontSize = profile.fontSize;
        else dialogueText.fontSize = narrationProfile.fontSize;

        if (nameText != null)
        {
            nameText.color = profile.nameColor;
            if (profile.font != null) nameText.font = profile.font;
        }

        dialogueText.ForceMeshUpdate();
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void PlayDialogueSound(string line)
    {
        AudioClip[] targetClips = (line.Length <= 50) ? clipsShort : (line.Length <= 70) ? clipsMedium : clipsLong;
        if (targetClips == null || targetClips.Length == 0) return;

        audioSource.clip = targetClips[Random.Range(0, targetClips.Length)];
        audioSource.Play();
    }

    void EndDialogue()
    {
        PlayerPrefs.SetInt("CommunicationCompleted", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(returnSceneName);
    }
}
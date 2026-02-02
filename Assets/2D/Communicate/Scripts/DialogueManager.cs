using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video; // [NEW] 비디오 기능을 위해 추가
using System.Collections;
using System.Text;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class DialogueManager : MonoBehaviour
{
    // --- 화자별 설정용 구조체 ---
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

    [Header("UI 컴포넌트 연결")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;

    [Header("배경 비디오 연결")]
    public VideoPlayer backgroundVideoPlayer; // [NEW] 여기에 VideoPlayer가 달린 RawImage를 넣으세요.

    [Header("선택지 UI 설정")]
    public Button[] optionButtons;
    public TextMeshProUGUI[] optionButtonTexts;

    [Header("화자별 스타일 설정")]
    public List<SpeakerProfile> speakerProfiles;
    public SpeakerProfile narrationProfile;

    [Header("팝업 UI 연결")]
    public GameObject pauseMenuUI;
    public GameObject logPanelUI;
    public TextMeshProUGUI logContentText;
    public ScrollRect logScrollRect;

    [Header("데이터 파일")]
    public TextAsset dialogueFile;

    [Header("설정")]
    public float typingSpeed = 0.05f;

    [Header("오디오")]
    public AudioClip[] clipsShort;
    public AudioClip[] clipsMedium;
    public AudioClip[] clipsLong;

    // 내부 변수
    private List<string> lines = new List<string>();
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isPaused = false;
    private bool isWaitingForChoice = false;
    private AudioSource audioSource;

    // 타자기 관련
    private bool isTyping = false;
    private string currentContent;
    private Coroutine typingCoroutine;

    // 로그용
    private StringBuilder logBuilder = new StringBuilder();

    // 선택지 로직 및 스탯 저장용
    private List<ChoiceData> currentChoices = new List<ChoiceData>();
    private Dictionary<string, int> gameStats = new Dictionary<string, int>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (logPanelUI != null) logPanelUI.SetActive(false);
        if (logContentText != null) logContentText.text = "";
        if (nameText != null) nameText.text = "";

        // 비디오 플레이어 확인 (시작 시 재생 보장)
        if (backgroundVideoPlayer != null && !backgroundVideoPlayer.isPlaying)
        {
            backgroundVideoPlayer.Play();
        }

        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                if (optionButtons[i] != null)
                {
                    optionButtons[i].gameObject.SetActive(false);
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                }
            }
        }

        if (dialogueFile != null)
        {
            lines.AddRange(dialogueFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries));
            isDialogueActive = true;
            ShowNextLine();
        }
        else
        {
            Debug.LogError("텍스트 파일이 연결되지 않았습니다!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (logPanelUI != null && logPanelUI.activeSelf) CloseLog();
            else { if (isPaused) ResumeGame(); else PauseGame(); }
        }

        if (isPaused) return;
        if (isWaitingForChoice) return;

        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentContent;
                isTyping = false;
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    // --- 일시정지 기능 (비디오 제어 추가됨) ---

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

        if (audioSource.isPlaying) audioSource.Pause();

        // [NEW] 비디오 일시정지
        if (backgroundVideoPlayer != null) backgroundVideoPlayer.Pause();
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        audioSource.UnPause();

        // [NEW] 비디오 다시 재생
        if (backgroundVideoPlayer != null) backgroundVideoPlayer.Play();
    }

    // --- 나머지 기능들 (기존과 동일) ---
    // (아래 내용들은 이전 코드와 100% 동일하므로 생략하지 않고 그대로 유지합니다)

    public void OpenLog()
    {
        if (logPanelUI != null)
        {
            logPanelUI.SetActive(true);
            isPaused = true;
            if (backgroundVideoPlayer != null) backgroundVideoPlayer.Pause(); // 로그 창 켤 때 비디오 멈춤
            StartCoroutine(AutoScrollToBottom());
        }
    }

    public void CloseLog()
    {
        if (logPanelUI != null)
        {
            logPanelUI.SetActive(false);
            isPaused = false;
            if (backgroundVideoPlayer != null) backgroundVideoPlayer.Play(); // 로그 창 닫으면 비디오 재생
        }
    }

    void ShowNextLine()
    {
        if (currentLineIndex >= lines.Count)
        {
            EndDialogue();
            return;
        }
        string rawLine = lines[currentLineIndex].Trim();
        if (rawLine.StartsWith("[ID:")) { currentLineIndex++; ShowNextLine(); return; }
        if (rawLine.StartsWith("[GOTO:")) { string targetID = rawLine.Replace("[GOTO:", "").Replace("]", "").Trim(); JumpToID(targetID); return; }
        if (rawLine.Equals("[CHOICE]")) { StartChoiceMode(); return; }
        ParseAndApplyStyle(rawLine, out string speaker, out string content);
        currentContent = content;
        string logLine = string.IsNullOrEmpty(speaker) ? content : $"<color=yellow>[{speaker}]</color> {content}";
        AddToLog(logLine);
        PlayDialogueSound(currentContent);
        typingCoroutine = StartCoroutine(TypeText(currentContent));
        currentLineIndex++;
    }

    void StartChoiceMode()
    {
        isWaitingForChoice = true;
        currentChoices.Clear();
        currentLineIndex++;
        int buttonIndex = 0;
        while (currentLineIndex < lines.Count && lines[currentLineIndex].Trim().StartsWith(">"))
        {
            string line = lines[currentLineIndex].Trim();
            string data = line.Substring(1).Trim();
            string[] parts = data.Split(new char[] { ':' });
            if (parts.Length >= 2 && buttonIndex < optionButtons.Length)
            {
                string btnText = parts[0].Trim();
                string targetID = parts[1].Trim();
                string statName = (parts.Length > 2) ? parts[2].Trim() : "";
                optionButtons[buttonIndex].gameObject.SetActive(true);
                if (optionButtonTexts[buttonIndex] != null) optionButtonTexts[buttonIndex].text = btnText;
                ChoiceData newChoice = new ChoiceData { targetID = targetID, statName = statName };
                currentChoices.Add(newChoice);
                buttonIndex++;
            }
            currentLineIndex++;
        }
    }

    public void OnOptionSelected(int index)
    {
        if (index >= currentChoices.Count) return;
        ChoiceData choice = currentChoices[index];
        if (!string.IsNullOrEmpty(choice.statName))
        {
            if (!gameStats.ContainsKey(choice.statName)) gameStats[choice.statName] = 0;
            gameStats[choice.statName]++;
            Debug.Log($"[Stat] '{choice.statName}' 수치 증가! 현재 값: {gameStats[choice.statName]}");
        }
        foreach (var btn in optionButtons) btn.gameObject.SetActive(false);
        isWaitingForChoice = false;
        JumpToID(choice.targetID);
    }

    void JumpToID(string targetID)
    {
        string searchTag = $"[ID:{targetID}]";
        bool found = false;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim().Equals(searchTag)) { currentLineIndex = i + 1; found = true; break; }
        }
        if (found) ShowNextLine();
        else Debug.LogError($"이동할 ID를 찾을 수 없습니다: {targetID}");
    }

    public int GetStatCount(string statName) { if (gameStats.ContainsKey(statName)) return gameStats[statName]; return 0; }
    void ParseAndApplyStyle(string rawLine, out string speakerName, out string content)
    {
        if (rawLine.Trim().StartsWith("[")) { speakerName = ""; content = rawLine.Trim(); if (nameText != null) nameText.text = ""; ApplyProfile(narrationProfile); return; }
        string[] parts = rawLine.Split(new char[] { ':' }, 2);
        if (parts.Length == 2) { speakerName = parts[0].Trim(); content = parts[1].Trim(); if (nameText != null) nameText.text = speakerName; ApplyStyleByName(speakerName); }
        else { speakerName = ""; content = rawLine.Trim(); if (nameText != null) nameText.text = ""; ApplyProfile(narrationProfile); }
    }
    void ApplyStyleByName(string name) { SpeakerProfile foundProfile = speakerProfiles.Find(x => x.speakerName == name); if (!string.IsNullOrEmpty(foundProfile.speakerName)) ApplyProfile(foundProfile); else { dialogueText.color = Color.white; if (nameText != null) nameText.color = Color.white; } }
    void ApplyProfile(SpeakerProfile profile) { dialogueText.color = profile.dialogueColor; if (profile.font != null) dialogueText.font = profile.font; if (profile.fontSize > 0) dialogueText.fontSize = profile.fontSize; if (nameText != null) nameText.color = profile.nameColor; }
    public void GoToHome() { Time.timeScale = 1f; Debug.Log("홈으로 이동"); }
    void AddToLog(string line) { if (logContentText != null) { logBuilder.AppendLine(line); logBuilder.AppendLine(""); logContentText.text = logBuilder.ToString(); StartCoroutine(AutoScrollToBottom()); } }
    IEnumerator AutoScrollToBottom() { yield return new WaitForEndOfFrame(); if (logScrollRect != null) logScrollRect.verticalNormalizedPosition = 0f; }
    IEnumerator TypeText(string line) { isTyping = true; dialogueText.text = ""; foreach (char letter in line.ToCharArray()) { dialogueText.text += letter; yield return new WaitForSeconds(typingSpeed); } isTyping = false; }
    void PlayDialogueSound(string line) { int length = line.Length; AudioClip[] targetClips = null; if (length <= 50) targetClips = clipsShort; else if (length <= 70) targetClips = clipsMedium; else targetClips = clipsLong; if (targetClips != null && targetClips.Length > 0) { int randomIndex = Random.Range(0, targetClips.Length); AudioClip randomClip = targetClips[randomIndex]; if (randomClip != null) { audioSource.Stop(); audioSource.clip = randomClip; audioSource.Play(); } } }
    void EndDialogue()
    {
        Debug.Log("대사가 끝났습니다."); if (audioSource != null) audioSource.Stop(); isDialogueActive = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
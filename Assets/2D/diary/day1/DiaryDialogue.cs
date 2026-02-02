using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DiaryDialogue : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text leftText;
    public TMP_Text rightText;
    public DialogueHistoryManager historyManager;

    [Header("Dialogue Settings")]
    public TextAsset[] dialogueFilesByDay; // Day 1~6 파일 연결
    public float typingSpeed = 0.05f;
    public int maxLinesPerPage = 20;

    [Header("Audio Settings")]
    public AudioSource sfxAudioSource;
    public AudioClip typingSound;
    [Range(0.8f, 1.2f)]
    public float pitchVariance = 0.1f;

    [Header("Scene Settings")]
    public string mainGameSceneName = "GameScene"; // 돌아갈 방 씬 이름

    private string[] lines;
    private int currentIndex = 0;
    private bool isTyping = false;
    private int currentDay = 1;

    private TMP_Text currentTargetUI;
    private string leftAccumulated = "";
    private string rightAccumulated = "";

    void Start()
    {
        currentTargetUI = leftText;
        leftText.text = "";
        rightText.text = "";

        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        Debug.Log($"[DiaryDialogue] Day {currentDay} 일기 시작");

        // Day 7 이상이면 일기장 켜지면 안 됨 -> 바로 복귀
        if (currentDay > 6)
        {
            ReturnToRoomOnly();
            return;
        }

        LoadDialogueForDay(currentDay);
    }

    void LoadDialogueForDay(int day)
    {
        int index = day - 1;

        if (dialogueFilesByDay == null || index < 0 || index >= dialogueFilesByDay.Length || dialogueFilesByDay[index] == null)
        {
            Debug.LogError($"[DiaryDialogue] Day {day} 파일 없음. 방으로 복귀.");
            ReturnToRoomOnly();
            return;
        }

        lines = dialogueFilesByDay[index].text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length > 0)
            StartCoroutine(TypeSentence(FormatText(lines[currentIndex])));
        else
            ReturnToRoomOnly();
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                FinishLine();
                isTyping = false;
            }
            else if (currentIndex < lines.Length - 1)
            {
                currentIndex++;
                CheckPage();
                StartCoroutine(TypeSentence(FormatText(lines[currentIndex])));
            }
            else
            {
                // ★ 마지막 대사 후 클릭 시
                OnDiaryComplete();
            }
        }
    }

    void OnDiaryComplete()
    {
        Debug.Log($"[DiaryDialogue] Day {currentDay} 일기 작성 완료. 방으로 돌아갑니다.");

        // ★ [핵심] 일기 작성 완료 플래그 저장
        PlayerPrefs.SetInt("DiaryCompleted", 1);
        PlayerPrefs.Save();

        // ★ [핵심] 잠자지 않고 방으로 이동
        SceneManager.LoadScene(mainGameSceneName);
    }

    void ReturnToRoomOnly()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }

    // --- 텍스트 출력 로직 (동일) ---
    string FormatText(string text) => text.Replace(" ", "\u00A0");

    void CheckPage()
    {
        currentTargetUI.ForceMeshUpdate();
        if (currentTargetUI.textInfo.lineCount >= maxLinesPerPage)
        {
            if (currentTargetUI == leftText) currentTargetUI = rightText;
            else ResetPages();
        }
    }

    void ResetPages()
    {
        leftAccumulated = ""; rightAccumulated = "";
        leftText.text = ""; rightText.text = "";
        currentTargetUI = leftText;
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        string baseText = (currentTargetUI == leftText) ? leftAccumulated : rightAccumulated;
        string currentLineProgress = "";

        if (sfxAudioSource != null) sfxAudioSource.pitch = 1f;

        foreach (char letter in sentence.ToCharArray())
        {
            currentLineProgress += letter;
            currentTargetUI.text = baseText + currentLineProgress;

            if (sfxAudioSource != null && typingSound != null && !char.IsWhiteSpace(letter))
            {
                sfxAudioSource.pitch = Random.Range(1f - pitchVariance, 1f + pitchVariance);
                sfxAudioSource.PlayOneShot(typingSound);
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        if (sfxAudioSource != null) sfxAudioSource.pitch = 1f;

        UpdateData(sentence);
        isTyping = false;
    }

    void FinishLine()
    {
        string sentence = FormatText(lines[currentIndex]);
        UpdateData(sentence);
        leftText.text = leftAccumulated;
        rightText.text = rightAccumulated;
    }

    void UpdateData(string sentence)
    {
        if (historyManager != null) historyManager.AddLog(lines[currentIndex]);
        if (currentTargetUI == leftText) leftAccumulated += sentence + "\n\n";
        else rightAccumulated += sentence + "\n\n";
    }
}
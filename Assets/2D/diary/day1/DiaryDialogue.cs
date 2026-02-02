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
    public TextAsset dialogueFile; // ★ 이 씬에 해당하는 대화 파일 (하나만)
    public float typingSpeed = 0.05f;
    public int maxLinesPerPage = 20;

    [Header("Audio Settings")]
    public AudioSource sfxAudioSource;
    public AudioClip typingSound;
    [Range(0.8f, 1.2f)]
    public float pitchVariance = 0.1f;

    [Header("Scene Settings")]
    public string mainGameSceneName = "GameScene"; // ★ 복귀할 3D 메인 씬 이름

    private string[] lines;
    private int currentIndex = 0;
    private bool isTyping = false;

    private TMP_Text currentTargetUI;
    private string leftAccumulated = "";
    private string rightAccumulated = "";

    void Start()
    {
        currentTargetUI = leftText;
        leftText.text = "";
        rightText.text = "";

        // 대화 파일 로드
        if (dialogueFile != null)
        {
            lines = dialogueFile.text.Split(
                new[] { "\r\n", "\r", "\n" },
                System.StringSplitOptions.RemoveEmptyEntries
            );

            if (lines.Length > 0)
            {
                StartCoroutine(TypeSentence(FormatText(lines[currentIndex])));
            }
            else
            {
                Debug.LogWarning("[DiaryDialogue] 대화 파일이 비어있습니다!");
                ReturnToMainGame();
            }
        }
        else
        {
            Debug.LogError("[DiaryDialogue] dialogueFile이 할당되지 않았습니다!");
            ReturnToMainGame();
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // 타이핑 중일 때 클릭하면 즉시 완성
                StopAllCoroutines();
                FinishLine();
                isTyping = false;
            }
            else if (currentIndex < lines.Length - 1)
            {
                // 다음 줄로 이동
                currentIndex++;
                CheckPage();
                StartCoroutine(TypeSentence(FormatText(lines[currentIndex])));
            }
            else
            {
                // ★ 마지막 대사 후 클릭 시 메인 게임으로 복귀 + 다음 날로
                OnDiaryComplete();
            }
        }
    }

    void OnDiaryComplete()
    {
        int currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        Debug.Log($"[DiaryDialogue] Day {currentDay} 일기 작성 완료! 다음 날로 진행");

        // ★ 일기 작성 완료 플래그 설정
        PlayerPrefs.SetInt("DiaryCompleted", 1);
        PlayerPrefs.Save();

        ReturnToMainGame();
    }

    void ReturnToMainGame()
    {
        Debug.Log($"[DiaryDialogue] {mainGameSceneName} 씬으로 복귀");
        SceneManager.LoadScene(mainGameSceneName);
    }

    string FormatText(string text)
    {
        // 공백을 Non-Breaking Space로 변경 (줄바꿈 방지)
        return text.Replace(" ", "\u00A0");
    }

    void CheckPage()
    {
        currentTargetUI.ForceMeshUpdate();

        if (currentTargetUI.textInfo.lineCount >= maxLinesPerPage)
        {
            if (currentTargetUI == leftText)
            {
                currentTargetUI = rightText;
            }
            else
            {
                ResetPages();
            }
        }
    }

    void ResetPages()
    {
        leftAccumulated = "";
        rightAccumulated = "";
        leftText.text = "";
        rightText.text = "";
        currentTargetUI = leftText;
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        string currentLineProgress = "";
        string baseText = (currentTargetUI == leftText) ? leftAccumulated : rightAccumulated;

        // 소리 피치 초기화
        if (sfxAudioSource != null)
        {
            sfxAudioSource.pitch = 1f;
        }

        foreach (char letter in sentence.ToCharArray())
        {
            currentLineProgress += letter;
            currentTargetUI.text = baseText + currentLineProgress;

            // 타이핑 사운드 재생 (공백 제외)
            if (sfxAudioSource != null && typingSound != null && !char.IsWhiteSpace(letter))
            {
                sfxAudioSource.pitch = Random.Range(1f - pitchVariance, 1f + pitchVariance);
                sfxAudioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // 피치 복구
        if (sfxAudioSource != null)
        {
            sfxAudioSource.pitch = 1f;
        }

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
        // 히스토리 매니저에 로그 추가
        if (historyManager != null)
        {
            historyManager.AddLog(lines[currentIndex]);
        }

        // 누적 텍스트에 추가
        if (currentTargetUI == leftText)
        {
            leftAccumulated += sentence + "\n\n";
        }
        else
        {
            rightAccumulated += sentence + "\n\n";
        }
    }
}
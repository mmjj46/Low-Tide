using UnityEngine;
using TMPro;
using UnityEngine.UI; // [중요] ScrollRect 등 UI 기능을 위해 추가
using System.Collections;
using System.Text;

[RequireComponent(typeof(AudioSource))]
public class DialogueManager : MonoBehaviour
{
    [Header("메인 UI 컴포넌트")]
    public TextMeshProUGUI dialogueText;
    public GameObject[] buttonsToHide;

    [Header("팝업 UI 연결")]
    public GameObject pauseMenuUI;
    public GameObject logPanelUI;
    public TextMeshProUGUI logContentText;
    public ScrollRect logScrollRect; // [NEW] 인스펙터에서 Scroll View를 연결하세요!

    [Header("데이터 파일")]
    public TextAsset dialogueFile;

    [Header("설정")]
    public float typingSpeed = 0.05f;

    [Header("오디오")]
    public AudioClip[] clipsShort;
    public AudioClip[] clipsMedium;
    public AudioClip[] clipsLong;

    // 내부 변수
    private string[] lines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isPaused = false;
    private AudioSource audioSource;

    // 타자기 관련
    private bool isTyping = false;
    private string currentFullLine;
    private Coroutine typingCoroutine;

    // 로그 저장용
    private StringBuilder logBuilder = new StringBuilder();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (logPanelUI != null) logPanelUI.SetActive(false);
        if (logContentText != null) logContentText.text = "";

        if (buttonsToHide != null)
        {
            foreach (GameObject btn in buttonsToHide)
            {
                if (btn != null) btn.SetActive(false);
            }
        }

        if (dialogueFile != null)
        {
            lines = dialogueFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
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
            if (logPanelUI != null && logPanelUI.activeSelf)
            {
                CloseLog();
            }
            else
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }

        if (isPaused) return;

        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentFullLine;
                isTyping = false;
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    // --- 로그 기능 ---

    public void OpenLog()
    {
        if (logPanelUI != null)
        {
            logPanelUI.SetActive(true);
            isPaused = true;

            // [추가] 로그 창을 열 때도 스크롤을 맨 아래로
            StartCoroutine(AutoScrollToBottom());
        }
    }

    public void CloseLog()
    {
        if (logPanelUI != null)
        {
            logPanelUI.SetActive(false);
            isPaused = false;
        }
    }

    // --- 일시정지 ---

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        if (audioSource.isPlaying) audioSource.Pause();
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        audioSource.UnPause();
    }

    public void GoToHome()
    {
        Time.timeScale = 1f;
        Debug.Log("홈으로 이동");
    }

    // --- 대사 처리 ---

    void ShowNextLine()
    {
        if (lines != null && currentLineIndex < lines.Length)
        {
            currentFullLine = lines[currentLineIndex];

            AddToLog(currentFullLine); // 로그 추가

            PlayDialogueSound(currentFullLine);
            typingCoroutine = StartCoroutine(TypeText(currentFullLine));
            currentLineIndex++;
        }
        else
        {
            EndDialogue();
        }
    }

    void AddToLog(string line)
    {
        if (logContentText != null)
        {
            logBuilder.AppendLine(line);
            logBuilder.AppendLine("");

            logContentText.text = logBuilder.ToString();

            // [핵심] 텍스트 추가 후 스크롤을 맨 아래로 내림
            StartCoroutine(AutoScrollToBottom());
        }
    }

    // UI가 갱신될 시간을 주기 위해 한 프레임 대기 후 스크롤 이동
    IEnumerator AutoScrollToBottom()
    {
        yield return new WaitForEndOfFrame();

        if (logScrollRect != null)
        {
            // VerticalNormalizedPosition: 1은 맨 위, 0은 맨 아래
            logScrollRect.verticalNormalizedPosition = 0f;

            // 혹시 그래도 안 되면 강제 레이아웃 재빌드 명령 사용 가능:
            // LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)logScrollRect.content);
        }
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void PlayDialogueSound(string line)
    {
        int length = line.Length;
        AudioClip[] targetClips = null;

        if (length <= 50) targetClips = clipsShort;
        else if (length <= 70) targetClips = clipsMedium;
        else targetClips = clipsLong;

        if (targetClips != null && targetClips.Length > 0)
        {
            int randomIndex = Random.Range(0, targetClips.Length);
            AudioClip randomClip = targetClips[randomIndex];

            if (randomClip != null)
            {
                audioSource.Stop();
                audioSource.clip = randomClip;
                audioSource.Play();
            }
        }
    }

    void EndDialogue()
    {
        Debug.Log("대사가 끝났습니다.");
        if (audioSource != null) audioSource.Stop();
        isDialogueActive = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
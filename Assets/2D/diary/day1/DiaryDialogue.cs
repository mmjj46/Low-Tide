using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class SpreadData
{
    public List<string> leftParagraphs = new List<string>();
    public List<string> rightParagraphs = new List<string>();
}

public class DiaryDialogue : MonoBehaviour
{
    [Header("--- Intro UI (오프닝 연출용) ---")]
    public GameObject mainDiaryUI;

    [Header("1. 표지 패널 (Cover Panel)")]
    public GameObject coverPanel;
    public Button coverUserNameButton;
    public TMP_Text coverUserNameText;

    [Header("2. 이름 입력 창 (Name Input Panel)")]
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button nameConfirmButton;

    [Header("--- UI Reference ---")]
    public TMP_Text leftText;
    public TMP_Text rightText;
    public DialogueHistoryManager historyManager;

    [Header("--- Pagination UI ---")]
    public Button prevButton;
    public Button nextButton;
    public Image[] pageDots;
    public Sprite dotOnSprite;
    public Sprite dotOffSprite;

    [Header("--- Dialogue Settings ---")]
    public TextAsset[] dialogueFilesByDay;
    public float typingSpeed = 0.05f;
    public int maxLinesPerPage = 20;

    [Header("--- Audio Settings ---")]
    public AudioSource sfxAudioSource;
    public AudioClip[] typingSounds;
    [Range(0.8f, 1.2f)]
    public float pitchVariance = 0.1f;

    [Header("--- Scene Settings ---")]
    public string mainGameSceneName = "GameScene";

    [Header("--- Exit Button ---")]
    public Button exitButton;

    private List<SpreadData> spreads = new List<SpreadData>();
    private int currentSpreadIndex = 0;
    private int maxSpreads = 0;
    private bool isTyping = false;
    private int currentDay = 1;
    private string playerName = "";

    private int currentParIndexInSpread = 0;
    private string displayedLeftText = "";
    private string displayedRightText = "";

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // ==========================================
        // 아래부터는 미션을 완료했을 때만 정상적으로 실행됩니다.
        // ==========================================

        if (mainDiaryUI != null) mainDiaryUI.SetActive(true);

        leftText.text = "";
        rightText.text = "";

        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (nextButton != null) nextButton.onClick.AddListener(NextAction);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);

        if (coverUserNameButton != null) coverUserNameButton.onClick.AddListener(OnCoverUserNameClicked);
        if (nameConfirmButton != null) nameConfirmButton.onClick.AddListener(OnNameConfirmClicked);

        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        Debug.Log($"[DiaryDialogue] Day {currentDay} 일기 시작/열람");

        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            ShowCoverPanel();
        }
        else
        {
            playerName = PlayerPrefs.GetString("PlayerName", "주인공");
            SkipIntroAndStartDiary();
        }
    }

    void ShowCoverPanel()
    {
        if (mainDiaryUI != null) mainDiaryUI.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        if (nameConfirmButton != null) nameConfirmButton.gameObject.SetActive(false);

        if (coverUserNameText != null) coverUserNameText.text = "User Name";
        if (coverPanel != null) coverPanel.SetActive(true);
    }

    public void OnCoverUserNameClicked()
    {
        if (nameInputPanel != null) nameInputPanel.SetActive(true);
        if (nameConfirmButton != null) nameConfirmButton.gameObject.SetActive(true);
    }

    public void OnNameConfirmClicked()
    {
        if (string.IsNullOrWhiteSpace(nameInputField.text)) return;

        playerName = nameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        if (nameConfirmButton != null) nameConfirmButton.gameObject.SetActive(false);

        StartCoroutine(ShowFinalCoverAndStart());
    }

    IEnumerator ShowFinalCoverAndStart()
    {
        if (coverUserNameText != null) coverUserNameText.text = playerName;
        if (coverPanel != null) coverPanel.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        if (coverPanel != null) coverPanel.SetActive(false);
        SkipIntroAndStartDiary();
    }

    void SkipIntroAndStartDiary()
    {
        if (coverPanel != null) coverPanel.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        if (nameConfirmButton != null) nameConfirmButton.gameObject.SetActive(false);

        if (mainDiaryUI != null) mainDiaryUI.SetActive(true);

        if (currentDay > dialogueFilesByDay.Length)
        {
            LoadAllPastDialogues();
        }
        else
        {
            LoadDialogueForDay(currentDay);
        }
    }

    void LoadAllPastDialogues()
    {
        List<string> allLines = new List<string>();

        for (int i = 0; i < dialogueFilesByDay.Length; i++)
        {
            if (dialogueFilesByDay[i] != null)
            {
                allLines.Add($"<color=#666666><b>[ Day {i + 1}의 기록 ]</b></color>");

                string[] lines = dialogueFilesByDay[i].text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                allLines.AddRange(lines);

                allLines.Add("\n");
            }
        }

        if (allLines.Count > 0)
        {
            PrecalculateSpreads(allLines.ToArray());
            ShowSpread(0);
        }
        else
        {
            ReturnToRoomOnly();
        }
    }

    void LoadDialogueForDay(int day)
    {
        int index = day - 1;

        if (dialogueFilesByDay == null || index < 0 || index >= dialogueFilesByDay.Length || dialogueFilesByDay[index] == null)
        {
            ReturnToRoomOnly();
            return;
        }

        string[] lines = dialogueFilesByDay[index].text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length > 0)
        {
            PrecalculateSpreads(lines);
            ShowSpread(0);
        }
        else
        {
            ReturnToRoomOnly();
        }
    }

    void PrecalculateSpreads(string[] lines)
    {
        if (leftText != null) leftText.gameObject.SetActive(true);
        if (rightText != null) rightText.gameObject.SetActive(true);

        spreads.Clear();
        SpreadData currentSpread = new SpreadData();

        string testLeftText = "";
        string testRightText = "";
        bool isLeftActive = true;

        leftText.text = "";
        rightText.text = "";

        foreach (string line in lines)
        {
            if (historyManager != null) historyManager.AddLog(line);

            string personalizedLine = line.Replace("{Name}", playerName);
            string formattedLine = personalizedLine.Replace(" ", "\u00A0") + "\n\n";

            TMP_Text activeUI = isLeftActive ? leftText : rightText;
            string currentTestText = isLeftActive ? testLeftText : testRightText;

            activeUI.text = currentTestText + formattedLine;
            activeUI.ForceMeshUpdate();

            if (activeUI.textInfo.lineCount > maxLinesPerPage)
            {
                if (isLeftActive)
                {
                    isLeftActive = false;
                    testRightText = formattedLine;
                    currentSpread.rightParagraphs.Add(formattedLine);
                }
                else
                {
                    spreads.Add(currentSpread);
                    currentSpread = new SpreadData();
                    isLeftActive = true;

                    testLeftText = formattedLine;
                    testRightText = "";
                    currentSpread.leftParagraphs.Add(formattedLine);
                }
            }
            else
            {
                if (isLeftActive)
                {
                    testLeftText += formattedLine;
                    currentSpread.leftParagraphs.Add(formattedLine);
                }
                else
                {
                    testRightText += formattedLine;
                    currentSpread.rightParagraphs.Add(formattedLine);
                }
            }
        }

        if (currentSpread.leftParagraphs.Count > 0 || currentSpread.rightParagraphs.Count > 0)
        {
            spreads.Add(currentSpread);
        }

        maxSpreads = spreads.Count;
        leftText.text = "";
        rightText.text = "";
    }

    void ShowSpread(int spreadIndex)
    {
        currentSpreadIndex = spreadIndex;
        currentParIndexInSpread = 0;
        displayedLeftText = "";
        displayedRightText = "";

        leftText.text = "";
        rightText.text = "";

        UpdateDots();

        if (prevButton != null) prevButton.gameObject.SetActive(currentSpreadIndex > 0);

        StopAllCoroutines();
        ShowNextParagraph();
    }

    void ShowNextParagraph()
    {
        if (currentSpreadIndex >= spreads.Count) return;

        SpreadData spread = spreads[currentSpreadIndex];
        int totalPars = spread.leftParagraphs.Count + spread.rightParagraphs.Count;

        if (currentParIndexInSpread >= totalPars)
        {
            NextPage();
            return;
        }

        bool isLeft = currentParIndexInSpread < spread.leftParagraphs.Count;

        string parToType = isLeft
            ? spread.leftParagraphs[currentParIndexInSpread]
            : spread.rightParagraphs[currentParIndexInSpread - spread.leftParagraphs.Count];

        TMP_Text targetUI = isLeft ? leftText : rightText;

        StartCoroutine(TypeParagraph(targetUI, isLeft, parToType));
    }

    void UpdateDots()
    {
        for (int i = 0; i < pageDots.Length; i++)
        {
            if (pageDots[i] == null) continue;

            if (i < maxSpreads)
            {
                pageDots[i].gameObject.SetActive(true);
                pageDots[i].sprite = (i == currentSpreadIndex) ? dotOnSprite : dotOffSprite;
            }
            else
            {
                pageDots[i].gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        // 미션이 완료되지 않아 mainDiaryUI가 꺼져있다면 Update의 입력 처리도 무시합니다.
        if (mainDiaryUI != null && !mainDiaryUI.activeSelf) return;

        if ((coverPanel != null && coverPanel.activeSelf) ||
            (nameInputPanel != null && nameInputPanel.activeSelf))
            return;

        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && !IsPointerOverUI()))
        {
            NextAction();
        }
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void NextAction()
    {
        if (isTyping) SkipTyping();
        else ShowNextParagraph();
    }

    public void NextPage()
    {
        if (currentSpreadIndex < maxSpreads - 1)
        {
            ShowSpread(currentSpreadIndex + 1);
        }
        else
        {
            OnDiaryComplete();
        }
    }

    public void PrevPage()
    {
        if (isTyping) { SkipTyping(); return; }

        if (currentSpreadIndex > 0)
        {
            ShowSpread(currentSpreadIndex - 1);
        }
    }

    void SkipTyping()
    {
        StopAllCoroutines();

        SpreadData spread = spreads[currentSpreadIndex];
        bool isLeft = currentParIndexInSpread < spread.leftParagraphs.Count;

        string parToType = isLeft
            ? spread.leftParagraphs[currentParIndexInSpread]
            : spread.rightParagraphs[currentParIndexInSpread - spread.leftParagraphs.Count];

        if (isLeft)
        {
            displayedLeftText += parToType;
            leftText.text = displayedLeftText;
        }
        else
        {
            displayedRightText += parToType;
            rightText.text = displayedRightText;
        }

        currentParIndexInSpread++;
        isTyping = false;

        if (sfxAudioSource != null)
        {
            sfxAudioSource.Stop();
            sfxAudioSource.pitch = 1f;
        }
    }

    IEnumerator TypeParagraph(TMP_Text targetUI, bool isLeft, string newText)
    {
        isTyping = true;
        string baseText = isLeft ? displayedLeftText : displayedRightText;
        string currentProgress = "";

        if (sfxAudioSource != null) sfxAudioSource.pitch = 1f;

        foreach (char letter in newText.ToCharArray())
        {
            currentProgress += letter;
            targetUI.text = baseText + currentProgress;

            if (sfxAudioSource != null && typingSounds != null && typingSounds.Length > 0 && !char.IsWhiteSpace(letter))
            {
                if (!sfxAudioSource.isPlaying)
                {
                    AudioClip randomClip = typingSounds[Random.Range(0, typingSounds.Length)];
                    sfxAudioSource.pitch = Random.Range(1f - pitchVariance, 1f + pitchVariance);
                    sfxAudioSource.clip = randomClip;
                    sfxAudioSource.Play();
                }
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        if (sfxAudioSource != null)
        {
            sfxAudioSource.Stop();
            sfxAudioSource.pitch = 1f;
        }

        if (isLeft) displayedLeftText += newText;
        else displayedRightText += newText;

        currentParIndexInSpread++;
        isTyping = false;
    }

    void OnDiaryComplete()
    {
        Debug.Log($"[DiaryDialogue] 일기 읽기/작성 완료. 방으로 돌아갑니다.");
        PlayerPrefs.SetInt("DiaryCompleted", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(mainGameSceneName);
    }

    public void OnExitButtonClicked()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }

    void ReturnToRoomOnly()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }
}
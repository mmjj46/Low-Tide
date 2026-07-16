using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuCanvas;

    // 👇 대화 기록 패널 추가
    public GameObject logPanel;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;

    private bool isPaused = false;

    void Awake()
    {
        // 씬 전환 시에도 유지 (코루틴 실행 보장)
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // ESC 키로 일시정지/재개
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    /// <summary>
    /// 새 게임 시작 - 모든 저장 데이터를 초기화하고 게임 씬으로 이동
    /// </summary>
    /// <param name="gameSceneName">로드할 게임 씬 이름</param>
    public void StartNewGame(string gameSceneName)
    {
        Debug.Log("========================================");
        Debug.Log("[SettingsManager] 새 게임 시작 프로세스 시작");
        Debug.Log("========================================");

        // 1. 물리적인 JSON 세이브 파일 삭제
        string savePath = Application.persistentDataPath + "/savefile.json";
        Debug.Log($"[SettingsManager] 세이브 파일 경로: {savePath}");

        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                Debug.Log("[SettingsManager] ✓ 세이브 파일 삭제 성공");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SettingsManager] ✗ 세이브 파일 삭제 실패: {e.Message}");
            }
        }

        // 2. 파일이 정말 삭제됐는지 재확인
        Debug.Log($"[SettingsManager] 파일 삭제 후 존재 여부: {File.Exists(savePath)}");

        // 3. ★★★ 새 게임 플래그를 파일로 저장 (PlayerPrefs는 신뢰 불가) ★★★
        string flagPath = Application.persistentDataPath + "/newgame.flag";
        try
        {
            File.WriteAllText(flagPath, "1");
            Debug.Log($"[SettingsManager] ✓ 새 게임 플래그 파일 생성: {flagPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SettingsManager] ✗ 플래그 파일 생성 실패: {e.Message}");
        }

        // 4. 게임 관련 PlayerPrefs 삭제 (보조적으로)
        PlayerPrefs.DeleteKey("NextDayPending");
        PlayerPrefs.DeleteKey("MiniGameTarget");
        PlayerPrefs.DeleteKey("MiniGameSuccess");
        PlayerPrefs.Save();

        // 5. 게임 상태 정상화
        Time.timeScale = 1f;
        isPaused = false;

        // 6. 게임 씬 로드
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log($"[SettingsManager] 씬 로드: {gameSceneName}");
            Debug.Log("========================================");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("[SettingsManager] ✗ 씬 이름이 비어있습니다!");
        }
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void Pause()
    {
        if (pauseMenuCanvas == null)
        {
            Debug.LogWarning("[SettingsManager] pauseMenuCanvas가 할당되지 않았습니다.");
            return;
        }

        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // 커서 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[SettingsManager] 게임 일시정지");
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void Resume()
    {
        if (pauseMenuCanvas == null)
        {
            Debug.LogWarning("[SettingsManager] pauseMenuCanvas가 할당되지 않았습니다.");
            return;
        }

        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // 커서 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[SettingsManager] 게임 재개");
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[SettingsManager] 게임 종료");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 전체화면 모드로 설정
    /// </summary>
    public void SetFullScreen()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        Debug.Log("[SettingsManager] 전체화면 모드 설정");
    }

    /// <summary>
    /// 창 모드로 설정
    /// </summary>
    public void SetWindowedMode()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        Debug.Log("[SettingsManager] 창 모드 설정");
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    /// <param name="volume">0.0001 ~ 1.0 범위의 볼륨</param>
    public void SetBGMVolume(float volume)
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("[SettingsManager] mainMixer가 할당되지 않았습니다.");
            return;
        }

        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("BGM", -80f); // 음소거
        }
        else
        {
            mainMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
        }
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    /// <param name="volume">0.0001 ~ 1.0 범위의 볼륨</param>
    public void SetSFXVolume(float volume)
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("[SettingsManager] mainMixer가 할당되지 않았습니다.");
            return;
        }

        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("SFX", -80f); // 음소거
        }
        else
        {
            mainMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        }
    }

    /// <summary>
    /// 일시정지 상태 확인
    /// </summary>
    public bool IsPaused() => isPaused;

    // ==========================================
    // 새로 추가된 버튼 기능들
    // ==========================================

    /// <summary>
    /// MAIN MENU 버튼: startscene으로 돌아가기
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("[SettingsManager] 메인 메뉴로 돌아갑니다.");
        SceneManager.LoadScene("startscene");
    }

    /// <summary>
    /// DIALOGUE HISTORY 버튼: Day7 씬의 logpanel 켜기
    /// </summary>
    public void OpenDialogueHistory()
    {
        if (logPanel != null)
        {
            logPanel.SetActive(true);
            Debug.Log("[SettingsManager] 대화 기록 패널을 켭니다.");
        }
        else
        {
            Debug.LogWarning("[SettingsManager] 인스펙터에 logPanel이 할당되지 않았습니다!");
        }
    }
}
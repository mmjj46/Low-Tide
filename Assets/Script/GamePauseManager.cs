using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GamePauseManager : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject pausePanel; // 하이어라키의 'Pause' 패널 (파란창)

    [Header("Audio Settings")]
    public AudioMixer mainMixer;  // OpeningAudioMixer 연결

    // 게임이 멈췄는지 확인하는 변수
    private bool isPaused = false;
    private float lastSfxPlayTime = 0f;

    private void Start()
    {
        // 게임 시작 시 패널 숨기기
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // 마우스 커서 숨기기 (1인칭/3인칭 게임인 경우)
        // 만약 마우스가 필요한 게임이면 이 줄은 지우세요.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // ESC 키를 누르면 일시정지/재개 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // =================================================================
    // ★ 일시정지 핵심 로직
    // =================================================================

    public void PauseGame()
    {
        isPaused = true;

        // 1. 패널 켜기
        if (pausePanel != null) pausePanel.SetActive(true);

        // 2. 시간 멈추기
        Time.timeScale = 0f;

        // 3. 마우스 커서 보이게 풀기
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GamePauseManager] 게임 일시정지");
    }

    public void ResumeGame()
    {
        isPaused = false;

        // 1. 패널 끄기
        if (pausePanel != null) pausePanel.SetActive(false);

        // 2. 시간 다시 흐르게 하기
        Time.timeScale = 1f;

        // 3. 마우스 커서 다시 잠그기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[GamePauseManager] 게임 재개");
    }

    // =================================================================
    // 아래는 타이틀 화면과 동일한 설정 기능들
    // =================================================================

    // 메인 메뉴(타이틀)로 돌아가기
    public void GoToMainMenu()
    {
        // 시간은 다시 흐르게 해두고 이동해야 함
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene"); // "StartScene"은 실제 씬 이름으로 변경!
    }

    public void SetWindowedMode()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        PlayUISound();
    }

    public void SetFullScreen()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        PlayUISound();
    }

    public void QuitGame()
    {
        PlayUISound();
        Application.Quit();
    }

    // --- 볼륨 조절 (타이틀 화면과 동일) ---
    public void SetBGMVolume(float volume)
    {
        if (mainMixer == null) return;
        if (volume <= 0.0001f) mainMixer.SetFloat("BGM", -80f);
        else mainMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer == null) return;
        if (volume <= 0.0001f) mainMixer.SetFloat("SFX", -80f);
        else mainMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);

        if (Time.unscaledTime - lastSfxPlayTime > 0.15f)
        {
            PlayUISound();
            lastSfxPlayTime = Time.unscaledTime;
        }
    }

    private void PlayUISound()
    {
        // SoundManager가 존재한다면 효과음 재생
        if (SoundManager.Instance != null && SoundManager.Instance.clickClip != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
        }
    }
}
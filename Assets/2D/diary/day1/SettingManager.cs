using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // 오디오 믹서를 쓰기 위해 필수!

public class SettingsManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuCanvas; // PauseMenu 전체 부모 오브젝트

    [Header("Audio Settings")]
    public AudioMixer mainMixer; // 인스펙터에서 MainMixer를 넣어주세요

    private bool isPaused = false;

    void Update()
    {
        // ESC 키를 누르면 켜져 있으면 끄고, 꺼져 있으면 킵니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0f; // 시간 멈춤
        isPaused = true;
    }

    // [설정창 닫기] 우측 하단 QUIT 버튼에 연결하세요.
    public void Resume()
    {
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f; // 시간 다시 흐름
        isPaused = false;
    }

    // [게임 종료] 화면 중앙 MAIN MENU 버튼에 이미 연결하신 함수 (그대로 유지)
    public void QuitGame()
    {
        Debug.Log("게임을 종료하거나 메인 화면으로 이동합니다.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // --- 해상도 설정 ---
    public void SetFullScreen() => Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    public void SetWindowedMode() => Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

    // --- 오디오 설정 (슬라이더용) ---
    // 슬라이더 값이 0.0001 ~ 1 사이일 때 자연스럽게 조절됩니다.
    public void SetBGMVolume(float volume)
    {
        // 1. 디버그 로그로 현재 슬라이더 값 확인 (Console 창 확인 필수!)
        // 값이 0.0001 ~ 1 사이가 아니면 슬라이더 설정이 틀린 것입니다.
        Debug.Log($"슬라이더 값: {volume}");

        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("BGM", -80f);
        }
        else
        {
            // 2. 변환된 데시벨 값 확인
            // 정상이라면 -80 ~ 0 사이의 숫자가 나와야 합니다.
            float db = Mathf.Log10(volume) * 20;
            Debug.Log($"변환된 DB: {db}");

            mainMixer.SetFloat("BGM", db);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("SFX", -80f);
        }
        else
        {
            mainMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        }
    }
}
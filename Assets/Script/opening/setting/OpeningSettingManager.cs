using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OpeningSettingManager : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject settingsPanel;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;

    // ★ 소리 재생 쿨타임 변수 추가
    private float lastSfxPlayTime = 0f;

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf) CloseSettings();
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        // 패널 열릴 때 소리 내고 싶으면 아래 주석 해제
        // PlayUISound(); 
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        PlayUISound(); // 닫기 버튼 소리
    }

    // --- 해상도 & 언어 ---
    public void SetWindowedMode()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        PlayUISound(); // 클릭 소리
    }

    public void SetFullScreen()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        PlayUISound(); // 클릭 소리
    }

    public void SetLanguageEnglish() { PlayUISound(); }
    public void SetLanguageKorean() { PlayUISound(); }
    public void QuitGame() { PlayUISound(); Application.Quit(); }

    // --- 볼륨 조절 ---

    public void SetBGMVolume(float volume)
    {
        if (mainMixer == null) return;
        if (volume <= 0.0001f) mainMixer.SetFloat("BGM", -80f);
        else mainMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer == null) return;

        // 1. 믹서 볼륨 조절
        if (volume <= 0.0001f) mainMixer.SetFloat("SFX", -80f);
        else mainMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);

        // 2. ★ 소리 크기 확인용 재생 (0.15초마다 한 번씩만 재생)
        if (Time.unscaledTime - lastSfxPlayTime > 0.15f)
        {
            PlayUISound();
            lastSfxPlayTime = Time.unscaledTime;
        }
    }

    // ★ 편하게 쓰려고 만든 도우미 함수
    private void PlayUISound()
    {
        // SoundManager가 있고, 클릭음(clickClip)이 설정되어 있다면 재생
        if (SoundManager.Instance != null && SoundManager.Instance.clickClip != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
        }
    }
}
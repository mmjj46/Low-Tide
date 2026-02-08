using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OpeningSettingManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject settingsPanel;

    [Header("Resolution Buttons")]
    public Button windowedBtn;
    public Button fullScreenBtn;

    // ★ [수정됨] 이미지 컴포넌트가 아니라 '게임 오브젝트' 자체를 연결합니다.
    [Header("Windowed Button Objects")]
    public GameObject windowedOnObj;  // 창모드 켜짐 (ON) 상태의 오브젝트
    public GameObject windowedOffObj; // 창모드 꺼짐 (OFF) 상태의 오브젝트

    [Header("Full Screen Button Objects")]
    public GameObject fullScreenOnObj;  // 전체화면 켜짐 (ON) 상태의 오브젝트
    public GameObject fullScreenOffObj; // 전체화면 꺼짐 (OFF) 상태의 오브젝트

    [Header("Language Buttons")]
    public Button englishBtn;
    public Button koreanBtn;
    public Image englishBtnBg;
    public Image koreanBtnBg;

    [Header("Volume Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    public Color activeColor = new Color(0.5f, 0.5f, 0.5f);
    public Color inactiveColor = new Color(0f, 0f, 0f);

    void Start()
    {
        InitializeSettings();

        windowedBtn.onClick.AddListener(() => SetResolution(false));
        fullScreenBtn.onClick.AddListener(() => SetResolution(true));

        englishBtn.onClick.AddListener(() => SetLanguage("English"));
        koreanBtn.onClick.AddListener(() => SetLanguage("Korean"));

        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void InitializeSettings()
    {
        bool isFullScreen = Screen.fullScreen;
        UpdateResolutionUI(isFullScreen);

        string lang = PlayerPrefs.GetString("Language", "Korean");
        UpdateLanguageUI(lang);

        float bgmVol = PlayerPrefs.GetFloat("BGM_Volume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFX_Volume", 0.75f);

        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        PlayerPrefs.Save();
    }

    public void SetResolution(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        UpdateResolutionUI(isFullScreen);
    }

    // ★★★ [핵심 수정] 오브젝트를 껐다 켰다 하는 방식
    void UpdateResolutionUI(bool isFullScreen)
    {
        if (isFullScreen)
        {
            // 전체화면 모드일 때:
            // 전체화면 ON 켜기 / OFF 끄기
            if (fullScreenOnObj != null) fullScreenOnObj.SetActive(true);
            if (fullScreenOffObj != null) fullScreenOffObj.SetActive(false);

            // 창모드 ON 끄기 / OFF 켜기
            if (windowedOnObj != null) windowedOnObj.SetActive(false);
            if (windowedOffObj != null) windowedOffObj.SetActive(true);
        }
        else
        {
            // 창 모드일 때:
            // 전체화면 ON 끄기 / OFF 켜기
            if (fullScreenOnObj != null) fullScreenOnObj.SetActive(false);
            if (fullScreenOffObj != null) fullScreenOffObj.SetActive(true);

            // 창모드 ON 켜기 / OFF 끄기
            if (windowedOnObj != null) windowedOnObj.SetActive(true);
            if (windowedOffObj != null) windowedOffObj.SetActive(false);
        }
    }

    // (나머지 언어, 볼륨 관련 코드는 동일함)
    public void SetLanguage(string lang)
    {
        PlayerPrefs.SetString("Language", lang);
        UpdateLanguageUI(lang);
    }

    void UpdateLanguageUI(string lang)
    {
        if (lang == "English")
        {
            englishBtnBg.color = activeColor;
            koreanBtnBg.color = inactiveColor;
        }
        else
        {
            englishBtnBg.color = inactiveColor;
            koreanBtnBg.color = activeColor;
        }
    }

    public void SetBGMVolume(float value)
    {
        PlayerPrefs.SetFloat("BGM_Volume", value);
        if (audioMixer != null) audioMixer.SetFloat("BGM", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFX_Volume", value);
        if (audioMixer != null) audioMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum WeatherType
{
    Sunny,   // 맑음
    Rainy,   // 비
    Stormy,  // 폭풍
    Foggy    // 안개
}

public class WeatherManager : MonoBehaviour
{
    [Header("--- UI 이미지 ---")]
    public Image weatherIconUI;

    [Header("--- 낮 날씨 아이콘 ---")]
    public Sprite sunnySprite;
    public Sprite rainySprite;
    public Sprite stormySprite;
    public Sprite foggySprite;

    [Header("--- 밤 날씨 아이콘 ---")]
    public Sprite nightSprite;

    [Header("--- 날씨 사운드 ---")]
    public AudioClip sunnySound;
    public AudioClip rainySound;
    public AudioClip stormySound;
    public AudioClip foggySound;

    // 👇 추가: 밤 전용 배경음악(BGM) 슬롯
    public AudioClip nightSound;

    [Header("--- 날짜별 날씨 패턴 ---")]
    public WeatherType[] dailyWeatherPattern;

    private WeatherType currentWeather;
    private bool isNight = false;

    public void SetWeather(int day)
    {
        int index = (day - 1) % dailyWeatherPattern.Length;
        currentWeather = dailyWeatherPattern[index];

        Debug.Log($"Day {day} 날씨 변경: {currentWeather}");

        UpdateUI();
        UpdateSound(currentWeather);
    }

    public void SetNightMode(bool nightMode)
    {
        isNight = nightMode;
        UpdateUI();

        // 👇 추가: 낮/밤 상태가 바뀔 때 음악도 즉시 업데이트합니다.
        UpdateSound(currentWeather);
    }

    void UpdateUI()
    {
        if (weatherIconUI == null) return;

        // 밤이면 날씨 묻지도 따지지도 않고 무조건 밤 아이콘 1개로 고정!
        if (isNight)
        {
            weatherIconUI.sprite = nightSprite;
            return;
        }

        // 낮일 때만 날씨에 따라 아이콘을 바꿔줍니다.
        switch (currentWeather)
        {
            case WeatherType.Sunny: weatherIconUI.sprite = sunnySprite; break;
            case WeatherType.Rainy: weatherIconUI.sprite = rainySprite; break;
            case WeatherType.Stormy: weatherIconUI.sprite = stormySprite; break;
            case WeatherType.Foggy: weatherIconUI.sprite = foggySprite; break;
        }
    }

    void UpdateSound(WeatherType weather)
    {
        AudioClip clip = null;

        // 👇 추가: 밤 상태일 때는 날씨 BGM 대신 무조건 밤 BGM을 선택합니다.
        if (isNight)
        {
            clip = nightSound;
        }
        else
        {
            // 낮일 때만 날씨별 BGM 선택
            switch (weather)
            {
                case WeatherType.Sunny: clip = sunnySound; break;
                case WeatherType.Rainy: clip = rainySound; break;
                case WeatherType.Stormy: clip = stormySound; break;
                case WeatherType.Foggy: clip = foggySound; break;
            }
        }

        if (SoundManager.Instance != null)
        {
            // SoundManager 내부에 "동일한 오디오 클립이면 재생하지 않음" 로직이 있으므로
            // 밤에서 밤으로, 혹은 낮 동일 날씨에서 연달아 호출되어도 중복 재생되지 않고 자연스럽게 유지됩니다.
            SoundManager.Instance.PlayBGM(clip);
        }
        else
        {
            Debug.LogWarning("SoundManager가 씬에 없습니다! BGM을 재생할 수 없습니다.");
        }
    }
}
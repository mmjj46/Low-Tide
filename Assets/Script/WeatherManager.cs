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

    [Header("--- 날씨 아이콘 ---")]
    public Sprite sunnySprite;
    public Sprite rainySprite;
    public Sprite stormySprite;
    public Sprite foggySprite;

    [Header("--- 날씨 사운드 ---")]
    public AudioClip sunnySound;
    public AudioClip rainySound;
    public AudioClip stormySound;
    public AudioClip foggySound;

    public AudioSource bgmAudioSource;

    [Header("--- 날짜별 날씨 패턴 ---")]
    public WeatherType[] dailyWeatherPattern;

    // GameManager가 호출하는 함수
    public void SetWeather(int day)
    {
        int index = (day - 1) % dailyWeatherPattern.Length;
        WeatherType todayWeather = dailyWeatherPattern[index];

        Debug.Log($"Day {day} 날씨 변경: {todayWeather}");

        UpdateUI(todayWeather);
        UpdateSound(todayWeather);
    }

    void UpdateUI(WeatherType weather)
    {
        if (weatherIconUI == null) return;

        switch (weather)
        {
            case WeatherType.Sunny:
                weatherIconUI.sprite = sunnySprite;
                break;
            case WeatherType.Rainy:
                weatherIconUI.sprite = rainySprite;
                break;
            case WeatherType.Stormy:
                weatherIconUI.sprite = stormySprite;
                break;
            case WeatherType.Foggy:
                weatherIconUI.sprite = foggySprite;
                break;
        }
    }

    void UpdateSound(WeatherType weather)
    {
        if (bgmAudioSource == null) return;

        AudioClip clip = null;

        switch (weather)
        {
            case WeatherType.Sunny:
                clip = sunnySound;
                break;
            case WeatherType.Rainy:
                clip = rainySound;
                break;
            case WeatherType.Stormy:
                clip = stormySound;
                break;
            case WeatherType.Foggy:
                clip = foggySound;
                break;
        }

        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying) return;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = clip;

        if (clip != null)
            bgmAudioSource.Play();
    }
}

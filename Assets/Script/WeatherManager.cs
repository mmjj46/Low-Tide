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

    // [삭제됨] 이제 이 친구는 직접 스피커를 들고 다니지 않습니다.
    // public AudioSource bgmAudioSource; 

    [Header("--- 날짜별 날씨 패턴 ---")]
    public WeatherType[] dailyWeatherPattern;

    // GameManager가 호출하는 함수
    public void SetWeather(int day)
    {
        // 날씨 패턴 배열 범위 안에서 순환하도록 설정
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
        AudioClip clip = null;

        // 1. 날씨에 맞는 음악 고르기
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

        // 2. [변경] SoundManager에게 "이 음악 틀어줘" 라고 요청하기
        if (SoundManager.Instance != null)
        {
            // PlayBGM 함수 안에 "같은 노래면 다시 안 틈" 기능이 이미 들어있습니다.
            // 그래서 그냥 던져주기만 하면 알아서 처리합니다!
            SoundManager.Instance.PlayBGM(clip);
        }
        else
        {
            Debug.LogWarning("SoundManager가 씬에 없습니다! BGM을 재생할 수 없습니다.");
        }
    }
}
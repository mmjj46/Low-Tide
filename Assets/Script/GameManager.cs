using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI taskTextUI;
    public TextMeshProUGUI dayTextUI;

    [Header("Mission Settings")]
    [TextArea(2, 5)]
    public List<string> missionTextsByDay;

    [Header("Mission-Specific Objects")]
    public WaterPurifier waterPurifierObject;
    public FoodDevice foodDeviceObject;
    public Wall wallObject;
    public Pipe pipeObject;
    public Generator generatorObject;
    public Telescope telescopeObject;
    public Communicate communicateObject;
    public Lanton lantonObject;

    // ★ WeatherManager (이미지 + 사운드만 담당)
    [Header("Environment")]
    public WeatherManager weatherManager;

    [Header("Game State")]
    private int currentDay = 1;
    private bool isTodayMissionComplete = false;

    void Start()
    {
        currentDay = 1;
        isTodayMissionComplete = false;

        UpdateTaskUI();
        UpdateDayUI();
        CheckForNewDayEvents();

        // ▶ 첫 날 날씨 적용
        if (weatherManager != null)
            weatherManager.SetWeather(currentDay);
    }

    public void GoToNextDay()
    {
        if (!IsTodayMissionComplete())
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowNotification("오늘의 미션을 먼저 완료하세요!");
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("잠을 자서 다음 날로 넘어갑니다.", () =>
            {
                currentDay++;
                isTodayMissionComplete = false;

                CheckForNewDayEvents();

                // ▶ 날짜 변경 시 날씨 업데이트
                if (weatherManager != null)
                    weatherManager.SetWeather(currentDay);

                UpdateTaskUI();
                UpdateDayUI();
            });
        }
        else
        {
            // UIManager 없을 때를 위한 fallback
            currentDay++;
            isTodayMissionComplete = false;

            CheckForNewDayEvents();

            if (weatherManager != null)
                weatherManager.SetWeather(currentDay);

            UpdateTaskUI();
            UpdateDayUI();
        }
    }

    private void CheckForNewDayEvents()
    {
        if (currentDay == 1 || currentDay == 8)
            waterPurifierObject?.BreakPurifier();

        if (currentDay == 2 || currentDay == 9)
            foodDeviceObject?.BreakDevice();

        if (currentDay == 3 || currentDay == 10)
            wallObject?.BreakWall();

        if (currentDay == 4 || currentDay == 11)
            pipeObject?.BreakPipe();

        if (currentDay == 5 || currentDay == 12)
            generatorObject?.BreakGenerator();

        if (currentDay == 6 || currentDay == 13)
            telescopeObject?.BreakTelescope();

        if (currentDay == 7 || currentDay == 14)
            communicateObject?.BreakCommunicate();

        if (currentDay == 15)
            lantonObject?.BreakLanton();
    }

    public void OnDeviceFixed(string deviceName)
    {
        if (isTodayMissionComplete) return;

        bool missionMatch = false;

        if ((currentDay == 1 || currentDay == 8) && deviceName == "WaterPurifier") missionMatch = true;
        else if ((currentDay == 2 || currentDay == 9) && deviceName == "FoodDevice") missionMatch = true;
        else if ((currentDay == 3 || currentDay == 10) && deviceName == "Wall") missionMatch = true;
        else if ((currentDay == 4 || currentDay == 11) && deviceName == "Pipe") missionMatch = true;
        else if ((currentDay == 5 || currentDay == 12) && deviceName == "Generator") missionMatch = true;
        else if ((currentDay == 6 || currentDay == 13) && deviceName == "Telescope") missionMatch = true;
        else if ((currentDay == 7 || currentDay == 14) && deviceName == "Communicate") missionMatch = true;
        else if (currentDay == 15 && deviceName == "Lanton") missionMatch = true;

        if (missionMatch)
        {
            isTodayMissionComplete = true;
            UpdateTaskUI();
        }
    }

    private void UpdateTaskUI()
    {
        if (taskTextUI == null) return;

        int missionIndex = currentDay - 1;

        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count ||
            string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]))
        {
            taskTextUI.text = "모든 미션 완료!";
            return;
        }

        if (isTodayMissionComplete)
        {
            taskTextUI.text = "오늘의 임무 완료!\n(침낭에서 자서 다음 날로)";
            return;
        }

        string rawText = missionTextsByDay[missionIndex];
        string coloredText = rawText.Replace("목표:", "<color=#D5C790>목표:</color><color=#FFFFFF>");
        taskTextUI.text = coloredText;
    }

    private void UpdateDayUI()
    {
        if (dayTextUI != null)
            dayTextUI.text = "Day " + currentDay;
    }

    public int GetCurrentDay() => currentDay;

    public bool IsTodayMissionComplete()
    {
        int missionIndex = currentDay - 1;

        if (isTodayMissionComplete) return true;
        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count) return true;

        return string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]);
    }
}

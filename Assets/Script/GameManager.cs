using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

    [Header("Environment")]
    public WeatherManager weatherManager;

    [Header("Save Settings")]
    public Transform playerTransform;
    private string savePath;

    [Header("Game State")]
    private int currentDay = 1;
    private bool isTodayMissionComplete = false;

    void Awake()
    {
        savePath = Application.persistentDataPath + "/savefile.json";
    }

    void Start()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        // ★ 1단계: 미니게임 복귀 확인 및 처리
        string miniGameTarget = PlayerPrefs.GetString("MiniGameTarget", "");
        int miniGameSuccess = PlayerPrefs.GetInt("MiniGameSuccess", 0);

        if (!string.IsNullOrEmpty(miniGameTarget) && miniGameSuccess == 1)
        {
            Debug.Log($"[GameManager] 미니게임 복귀 감지: {miniGameTarget}");

            // 저장 파일 로드
            LoadGameData();

            // ★ 2프레임 대기 (LoadGameData의 RestoreObjectStates 완료 대기)
            yield return null;
            yield return null;

            // 해당 Device 수리 처리
            ProcessMiniGameReturn(miniGameTarget);

            // PlayerPrefs 초기화
            PlayerPrefs.SetString("MiniGameTarget", "");
            PlayerPrefs.SetInt("MiniGameSuccess", 0);
            PlayerPrefs.Save();
        }
        // ★ 2단계: 이어하기
        else if (PlayerPrefs.GetInt("IsLoadGame", 0) == 1)
        {
            Debug.Log("[GameManager] 이어하기");
            LoadGameData();
            PlayerPrefs.SetInt("IsLoadGame", 0);
            PlayerPrefs.Save();
        }
        // ★ 3단계: 새 게임
        else
        {
            Debug.Log("[GameManager] 새 게임 시작");
            StartNewGame();
        }
    }

    /// <summary>
    /// 미니게임 성공 후 해당 Device 수리 처리
    /// </summary>
    private void ProcessMiniGameReturn(string deviceName)
    {
        Debug.Log($"[ProcessMiniGameReturn] {deviceName} 수리 처리");

        switch (deviceName)
        {
            case "WaterPurifier":
                if (waterPurifierObject != null)
                {
                    waterPurifierObject.isBroken = true; // 강제로 고장 상태로
                    waterPurifierObject.ForceFixFromMiniGame();
                }
                break;
            case "FoodDevice":
                if (foodDeviceObject != null)
                {
                    foodDeviceObject.isBroken = true;
                    foodDeviceObject.ForceFixFromMiniGame();
                }
                break;
            case "Wall":
                if (wallObject != null)
                {
                    wallObject.isBroken = true;
                    wallObject.ForceFixFromMiniGame();
                }
                break;
            case "Pipe":
                if (pipeObject != null)
                {
                    pipeObject.isBroken = true;
                    pipeObject.ForceFixFromMiniGame();
                }
                break;
            case "Generator":
                if (generatorObject != null)
                {
                    generatorObject.isBroken = true;
                    generatorObject.ForceFixFromMiniGame();
                }
                break;
            case "Telescope":
                if (telescopeObject != null)
                {
                    telescopeObject.isBroken = true;
                    telescopeObject.ForceFixFromMiniGame();
                }
                break;
            case "Communicate":
                if (communicateObject != null)
                {
                    communicateObject.isBroken = true;
                    communicateObject.ForceFixFromMiniGame();
                }
                break;
            case "Lanton":
                if (lantonObject != null)
                {
                    lantonObject.isBroken = true;
                    lantonObject.ForceFixFromMiniGame();
                }
                break;
        }
    }

    void RefreshGameStat(bool triggerEvents = false)
    {
        UpdateTaskUI();
        UpdateDayUI();

        if (triggerEvents)
        {
            CheckForNewDayEvents();
        }

        if (weatherManager != null)
            weatherManager.SetWeather(currentDay);
    }

    public void SaveGameData()
    {
        if (playerTransform == null)
        {
            Debug.LogError("GameManager에 플레이어가 연결되지 않았습니다!");
            return;
        }

        try
        {
            SaveData data = new SaveData(
                currentDay,
                isTodayMissionComplete,
                playerTransform.position,
                waterPurifierObject?.isBroken ?? false,
                foodDeviceObject?.isBroken ?? false,
                wallObject?.isBroken ?? false,
                pipeObject?.isBroken ?? false,
                generatorObject?.isBroken ?? false,
                telescopeObject?.isBroken ?? false,
                communicateObject?.isBroken ?? false,
                lantonObject?.isBroken ?? false
            );

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);

            Debug.Log($"[저장] Day {currentDay}, 미션완료: {isTodayMissionComplete}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
        }
    }

    public void LoadGameData()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("세이브 파일 없음. 새 게임 시작.");
            StartNewGame();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("세이브 데이터 파싱 실패!");
                StartNewGame();
                return;
            }

            currentDay = data.day;
            isTodayMissionComplete = data.isMissionComplete;

            Debug.Log($"[로드] Day {currentDay}, 미션완료: {isTodayMissionComplete}");

            if (playerTransform != null)
            {
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    playerTransform.position = data.playerPos;
                    cc.enabled = true;
                }
                else
                {
                    playerTransform.position = data.playerPos;
                }
            }

            RestoreObjectStates(data);
            RefreshGameStat(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로드 실패: {e.Message}");
            StartNewGame();
        }
    }

    private void RestoreObjectStates(SaveData data)
    {
        if (data.waterPurifierBroken)
            waterPurifierObject?.BreakPurifier();

        if (data.foodDeviceBroken)
            foodDeviceObject?.BreakDevice();

        if (data.wallBroken)
            wallObject?.BreakWall();

        if (data.pipeBroken)
            pipeObject?.BreakPipe();

        if (data.generatorBroken)
            generatorObject?.BreakGenerator();

        if (data.telescopeBroken)
            telescopeObject?.BreakTelescope();

        if (data.communicateBroken)
            communicateObject?.BreakCommunicate();

        if (data.lantonBroken)
            lantonObject?.BreakLanton();

        Debug.Log("오브젝트 상태 복원 완료");
    }

    private void StartNewGame()
    {
        currentDay = 1;
        isTodayMissionComplete = false;
        RefreshGameStat(true);
    }

    public void GoToNextDay()
    {
        if (!IsTodayMissionComplete())
        {
            UIManager.Instance?.ShowNotification("오늘의 미션을 먼저 완료하세요!");
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("잠을 자서 다음 날로 넘어갑니다.", () =>
            {
                currentDay++;
                isTodayMissionComplete = false;
                RefreshGameStat(true);
                SaveGameData();
            });
        }
        else
        {
            currentDay++;
            isTodayMissionComplete = false;
            RefreshGameStat(true);
            SaveGameData();
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
        Debug.Log($"[OnDeviceFixed] {deviceName}, Day={currentDay}, 완료={isTodayMissionComplete}");

        if (isTodayMissionComplete) return;

        bool missionMatch = CheckMissionMatch(deviceName);

        if (missionMatch)
        {
            isTodayMissionComplete = true;
            UpdateTaskUI();
            SaveGameData();

            Debug.Log($"{deviceName} 수리 완료! 미션 달성!");
        }
    }

    private bool CheckMissionMatch(string deviceName)
    {
        return (currentDay == 1 || currentDay == 8) && deviceName == "WaterPurifier" ||
               (currentDay == 2 || currentDay == 9) && deviceName == "FoodDevice" ||
               (currentDay == 3 || currentDay == 10) && deviceName == "Wall" ||
               (currentDay == 4 || currentDay == 11) && deviceName == "Pipe" ||
               (currentDay == 5 || currentDay == 12) && deviceName == "Generator" ||
               (currentDay == 6 || currentDay == 13) && deviceName == "Telescope" ||
               (currentDay == 7 || currentDay == 14) && deviceName == "Communicate" ||
               currentDay == 15 && deviceName == "Lanton";
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

        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count)
            return true;

        return string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]);
    }

    public void ManualSave()
    {
        SaveGameData();
        UIManager.Instance?.ShowNotification("게임이 저장되었습니다!");
        Debug.Log("수동 저장 완료!");
    }
}
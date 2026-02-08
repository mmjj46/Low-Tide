using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    private int currentDay = 1;
    private bool isTodayMissionComplete = false;

    [Header("--- Dev Test (Editor Only) ---")]
    [Range(1, 15)]
    public int debugStartDay = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/savefile.json";
    }

    void Start()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        // 0. ★★★ 새 게임 플래그 파일 확인 (최우선) ★★★
        string flagPath = Application.persistentDataPath + "/newgame.flag";
        Debug.Log($"[GameManager] 새 게임 플래그 파일 확인: {flagPath}");

        if (File.Exists(flagPath))
        {
            Debug.Log("[GameManager] ✓✓✓ 새 게임 플래그 감지! 강제 새 게임 시작 ✓✓✓");

            // 플래그 파일 삭제
            try
            {
                File.Delete(flagPath);
                Debug.Log("[GameManager] 플래그 파일 삭제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] 플래그 파일 삭제 실패: {e.Message}");
            }

            // 혹시 남은 세이브 파일도 삭제
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("[GameManager] 남아있던 세이브 파일 삭제");
            }

            StartNewGame();
            yield break;
        }

        // 1. 다음 날 진행 플래그 확인
        if (PlayerPrefs.GetInt("NextDayPending", 0) == 1)
        {
            LoadGameData();
            yield return null;
            currentDay++;
            isTodayMissionComplete = false;
            RefreshGameStat(true);
            SaveGameData();
            PlayerPrefs.SetInt("NextDayPending", 0);
            PlayerPrefs.Save();
            yield break;
        }

        // 2. 미니게임 복귀 확인
        string miniGameTarget = PlayerPrefs.GetString("MiniGameTarget", "");
        int miniGameSuccess = PlayerPrefs.GetInt("MiniGameSuccess", 0);

        if (!string.IsNullOrEmpty(miniGameTarget) && miniGameSuccess == 1)
        {
            LoadGameData();
            // UI 업데이트를 위한 2프레임 대기
            yield return null;
            yield return null;
            ProcessMiniGameReturn(miniGameTarget);
            PlayerPrefs.SetString("MiniGameTarget", "");
            PlayerPrefs.SetInt("MiniGameSuccess", 0);
            PlayerPrefs.Save();
            yield break;
        }

        // 3. 이어하기 확인 (파일이 실제로 있을 때만)
        Debug.Log($"[GameManager] 세이브 파일 존재 여부: {File.Exists(savePath)}");
        if (File.Exists(savePath))
        {
            Debug.Log("[GameManager] 기존 데이터 로드 중...");
            LoadGameData();
        }
        else
        {
            Debug.Log("[GameManager] 세이브 파일 없음 -> 새 게임 시작");
            StartNewGame();
        }
    }

    private void ProcessMiniGameReturn(string deviceName)
    {
        switch (deviceName)
        {
            case "WaterPurifier": waterPurifierObject?.ForceFixFromMiniGame(); break;
            case "FoodDevice": foodDeviceObject?.ForceFixFromMiniGame(); break;
            case "Wall": wallObject?.ForceFixFromMiniGame(); break;
            case "Pipe": pipeObject?.ForceFixFromMiniGame(); break;
            case "Generator": generatorObject?.ForceFixFromMiniGame(); break;
            case "Telescope": telescopeObject?.ForceFixFromMiniGame(); break;
            case "Communicate": communicateObject?.ForceFixFromMiniGame(); break;
            case "Lanton": lantonObject?.ForceFixFromMiniGame(); break;
        }
    }

    void RefreshGameStat(bool triggerEvents = false)
    {
        UpdateTaskUI();
        UpdateDayUI();
        if (triggerEvents) CheckForNewDayEvents();
        if (weatherManager != null) weatherManager.SetWeather(currentDay);
    }

    public void SaveGameData()
    {
        // playerTransform이 null이면 찾기 시도
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("[GameManager] Player Transform을 찾을 수 없습니다. 저장 취소.");
                return;
            }
        }

        try
        {
            SaveData data = new SaveData(
                currentDay, isTodayMissionComplete, playerTransform.position,
                waterPurifierObject?.isBroken ?? false, foodDeviceObject?.isBroken ?? false,
                wallObject?.isBroken ?? false, pipeObject?.isBroken ?? false,
                generatorObject?.isBroken ?? false, telescopeObject?.isBroken ?? false,
                communicateObject?.isBroken ?? false, lantonObject?.isBroken ?? false
            );

            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
            Debug.Log($"[GameManager] 게임 저장 완료 - Day {currentDay}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 저장 실패: {e.Message}");
        }
    }

    public void LoadGameData()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[GameManager] 세이브 파일 없음 - 새 게임 시작");
            StartNewGame();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            currentDay = data.day;
            isTodayMissionComplete = data.isMissionComplete;

            // 플레이어 위치 복원
            if (playerTransform != null)
            {
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                playerTransform.position = data.playerPos;
                if (cc != null) cc.enabled = true;
            }

            RestoreObjectStates(data);
            RefreshGameStat(false);
            Debug.Log($"[GameManager] 게임 로드 완료 - Day {currentDay}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 로드 실패: {e.Message}");
            StartNewGame();
        }
    }

    private void RestoreObjectStates(SaveData data)
    {
        if (data.waterPurifierBroken && waterPurifierObject != null) waterPurifierObject.BreakPurifier();
        if (data.foodDeviceBroken && foodDeviceObject != null) foodDeviceObject.BreakDevice();
        if (data.wallBroken && wallObject != null) wallObject.BreakWall();
        if (data.pipeBroken && pipeObject != null) pipeObject.BreakPipe();
        if (data.generatorBroken && generatorObject != null) generatorObject.BreakGenerator();
        if (data.telescopeBroken && telescopeObject != null) telescopeObject.BreakTelescope();
        if (data.communicateBroken && communicateObject != null) communicateObject.BreakCommunicate();
        if (data.lantonBroken && lantonObject != null) lantonObject.BreakLanton();
    }

    private void StartNewGame()
    {
        // ★ 수정: debugStartDay는 에디터 테스트용으로만 사용
#if UNITY_EDITOR
        currentDay = debugStartDay;
        Debug.Log($"[GameManager - DEV MODE] Day {debugStartDay}부터 시작");
#else
        currentDay = 1;  // 빌드에서는 항상 1일차부터 시작
        Debug.Log("[GameManager] 새 게임 시작 - Day 1");
#endif

        isTodayMissionComplete = false;
        RefreshGameStat(true);
        SaveGameData();
    }

    private void CheckForNewDayEvents()
    {
        // 각 날짜에 맞는 기기 고장 발생
        if (currentDay == 1 || currentDay == 8) waterPurifierObject?.BreakPurifier();
        if (currentDay == 2 || currentDay == 9) foodDeviceObject?.BreakDevice();
        if (currentDay == 3 || currentDay == 10) wallObject?.BreakWall();
        if (currentDay == 4 || currentDay == 11) pipeObject?.BreakPipe();
        if (currentDay == 5 || currentDay == 12) generatorObject?.BreakGenerator();
        if (currentDay == 6 || currentDay == 13) telescopeObject?.BreakTelescope();
        if (currentDay == 7 || currentDay == 14) communicateObject?.BreakCommunicate();
        if (currentDay == 15) lantonObject?.BreakLanton();
    }

    public void OnDeviceFixed(string deviceName)
    {
        if (isTodayMissionComplete) return;

        if (CheckMissionMatch(deviceName))
        {
            isTodayMissionComplete = true;
            UpdateTaskUI();
            SaveGameData();
            UIManager.Instance?.ShowNotification("오늘의 주요 수리를 완료했다.\n일기를 쓰고 쉴 수 있다.");
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

        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count || string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]))
        {
            taskTextUI.text = "<color=#FFFFFF>모든 생존 임무 완료!</color>";
            return;
        }

        if (isTodayMissionComplete)
        {
            taskTextUI.text = "<color=#FFFFFF>목표: 일기 작성</color>";
            return;
        }

        taskTextUI.text = $"<color=#FFFFFF>{missionTextsByDay[missionIndex]}</color>";
    }

    private void UpdateDayUI()
    {
        if (dayTextUI != null) dayTextUI.text = $"<color=#FFFFFF>DAY {currentDay}</color>";
    }

    public int GetCurrentDay() => currentDay;

    /// <summary>
    /// 오늘의 미션 완료 여부 반환
    /// </summary>
    public bool IsTodayMissionComplete() => isTodayMissionComplete;
}
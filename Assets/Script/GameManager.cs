using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 추가

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

    [Header("--- Dev Test ---")]
    [Range(1, 15)]
    public int debugStartDay = 1;

    void Awake()
    {
        // 싱글톤 패턴 적용
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
        // =========================================================
        // ★ 1단계: "침낭에서 자고 다음 날로 넘어가는 경우" 확인
        // =========================================================
        if (PlayerPrefs.GetInt("NextDayPending", 0) == 1)
        {
            Debug.Log("[GameManager] 침낭에서 잠 -> 다음 날로 진행 중...");

            // 1. 기존 데이터 로드
            LoadGameData();
            yield return null;

            // 2. 날짜 변경 및 초기화
            currentDay++;
            isTodayMissionComplete = false;

            // 3. 변경된 날짜로 UI 및 이벤트 갱신
            RefreshGameStat(true);

            // 4. 저장 (Day가 오른 상태로 저장)
            SaveGameData();

            // 5. 플래그 초기화
            PlayerPrefs.SetInt("NextDayPending", 0);
            PlayerPrefs.Save();

            Debug.Log($"[GameManager] Day {currentDay} 시작 완료!");
            yield break;
        }

        // =========================================================
        // ★ 2단계: "미니게임에서 수리하고 돌아온 경우" 확인
        // =========================================================
        string miniGameTarget = PlayerPrefs.GetString("MiniGameTarget", "");
        int miniGameSuccess = PlayerPrefs.GetInt("MiniGameSuccess", 0);

        if (!string.IsNullOrEmpty(miniGameTarget) && miniGameSuccess == 1)
        {
            Debug.Log($"[GameManager] 미니게임 복귀 감지: {miniGameTarget} 수리 성공");

            LoadGameData();

            // 오브젝트 상태 복원 대기
            yield return null;
            yield return null;

            ProcessMiniGameReturn(miniGameTarget);

            // 플래그 초기화
            PlayerPrefs.SetString("MiniGameTarget", "");
            PlayerPrefs.SetInt("MiniGameSuccess", 0);
            PlayerPrefs.Save();

            yield break;
        }

        // =========================================================
        // ★ 3단계: "이어하기" 또는 "일기장 복귀"
        // =========================================================
        if (PlayerPrefs.GetInt("IsLoadGame", 0) == 1 || File.Exists(savePath))
        {
            Debug.Log("[GameManager] 기존 데이터 이어하기 (일기장 복귀 포함)");

            LoadGameData();

            if (PlayerPrefs.GetInt("IsLoadGame", 0) == 1)
            {
                PlayerPrefs.SetInt("IsLoadGame", 0);
                PlayerPrefs.Save();
            }
        }
        // =========================================================
        // ★ 4단계: 완전한 새 게임
        // =========================================================
        else
        {
            Debug.Log("[GameManager] 저장 파일 없음 -> 새 게임 시작");
            StartNewGame();
        }
    }

    private void ProcessMiniGameReturn(string deviceName)
    {
        Debug.Log($"[ProcessMiniGameReturn] {deviceName} 수리 적용");

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
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else
            {
                Debug.LogError("GameManager: 플레이어를 찾을 수 없어 저장 실패.");
                return;
            }
        }

        try
        {
            // SaveData 생성자 호출 (매개변수 순서 주의)
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

            Debug.Log($"[저장 완료] Day {currentDay}, 파일 위치: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"저장 중 오류 발생: {e.Message}");
        }
    }

    public void LoadGameData()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("불러올 세이브 파일이 없습니다. 새 게임을 시작합니다.");
            StartNewGame();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("데이터 파일이 손상되었습니다.");
                StartNewGame();
                return;
            }

            currentDay = data.day;
            isTodayMissionComplete = data.isMissionComplete;

            Debug.Log($"[로드 완료] Day {currentDay}");

            if (playerTransform != null)
            {
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                playerTransform.position = data.playerPos;
                if (cc != null) cc.enabled = true;
            }

            RestoreObjectStates(data);
            RefreshGameStat(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로드 중 오류 발생: {e.Message}");
            StartNewGame();
        }
    }

    private void RestoreObjectStates(SaveData data)
    {
        if (data.waterPurifierBroken && waterPurifierObject != null) waterPurifierObject.BreakPurifier();
        else if (waterPurifierObject != null) waterPurifierObject.isBroken = false;

        if (data.foodDeviceBroken && foodDeviceObject != null) foodDeviceObject.BreakDevice();
        else if (foodDeviceObject != null) foodDeviceObject.isBroken = false;

        if (data.wallBroken && wallObject != null) wallObject.BreakWall();
        else if (wallObject != null) wallObject.isBroken = false;

        if (data.pipeBroken && pipeObject != null) pipeObject.BreakPipe();
        else if (pipeObject != null) pipeObject.isBroken = false;

        if (data.generatorBroken && generatorObject != null) generatorObject.BreakGenerator();
        else if (generatorObject != null) generatorObject.isBroken = false;

        if (data.telescopeBroken && telescopeObject != null) telescopeObject.BreakTelescope();
        else if (telescopeObject != null) telescopeObject.isBroken = false;

        if (data.communicateBroken && communicateObject != null) communicateObject.BreakCommunicate();
        else if (communicateObject != null) communicateObject.isBroken = false;

        if (data.lantonBroken && lantonObject != null) lantonObject.BreakLanton();
        else if (lantonObject != null) lantonObject.isBroken = false;
    }

    private void StartNewGame()
    {
        currentDay = debugStartDay;
        isTodayMissionComplete = false;

        RefreshGameStat(true);
        SaveGameData();

        Debug.Log($"[새 게임 시작] Day {currentDay}");
    }

    private void CheckForNewDayEvents()
    {
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
        Debug.Log($"[OnDeviceFixed] {deviceName} 수리됨. (현재 Day: {currentDay})");

        if (isTodayMissionComplete) return;

        bool missionMatch = CheckMissionMatch(deviceName);

        if (missionMatch)
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

        // 1. 모든 미션 완료 시
        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count ||
            string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]))
        {
            // [수정] 초록색(#00FF00) -> 흰색(#FFFFFF)으로 변경
            taskTextUI.text = "<color=#FFFFFF>모든 생존 임무 완료!</color>";
            return;
        }

        // 2. 오늘 미션 완료 상태일 때
        if (isTodayMissionComplete)
        {
            // [수정] 초록색(#00FF00) -> 흰색(#FFFFFF)으로 변경
            // 시안에 맞춰 깔끔하게 흰색으로 표시
            taskTextUI.text = $"<color=#FFFFFF>Day {currentDay} 임무 완료</color>\n<size=80%>(책상에서 일기를 작성하세요)</size>";
            return;
        }

        // 3. 미션 진행 중일 때 (기본 상태)
        string rawText = missionTextsByDay[missionIndex];

        // [수정] "목표:" 부분도 노란색이 아니라 시안대로 '흰색(#FFFFFF)'으로 통일
        // 만약 "목표:" 부분만 색을 다르게 하고 싶다면 #FFFFFF 부분을 다른 색 코드로 바꾸세요.
        string coloredText = rawText.Replace("목표:", "<color=#FFFFFF>목표:</color><color=#FFFFFF>");

        // 혹시 모르니 전체를 흰색으로 감싸줍니다.
        taskTextUI.text = $"<color=#FFFFFF>{coloredText}</color>";
    }

    private void UpdateDayUI()
    {
        if (dayTextUI != null)
        {
            // [수정] 디자인 시안(#FFFFFF)에 맞춰 흰색 강제 적용
            dayTextUI.text = $"<color=#FFFFFF>DAY {currentDay}</color>";
        }
    }

    public int GetCurrentDay() => currentDay;

    public bool IsTodayMissionComplete()
    {
        if (isTodayMissionComplete) return true;

        int missionIndex = currentDay - 1;
        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count) return true;

        return string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]);
    }
}
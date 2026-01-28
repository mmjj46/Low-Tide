using UnityEngine;
using TMPro;
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
    public Transform playerTransform; // Inspector에서 플레이어를 넣어주세요!
    private string savePath;

    [Header("Game State")]
    private int currentDay = 1;
    private bool isTodayMissionComplete = false;
    private bool isLoadingGame = false; // ★ 로드 중 플래그

    void Start()
    {
        // 1. 저장 경로 설정
        savePath = Application.persistentDataPath + "/savefile.json";

        // 2. StartScene에서 "이어하기" 신호를 보냈는지 확인
        int isLoad = PlayerPrefs.GetInt("IsLoadGame", 0);

        if (isLoad == 1)
        {
            // 이어하기라면 저장된 파일을 불러옴
            LoadGameData();
            PlayerPrefs.SetInt("IsLoadGame", 0); // ★ 플래그 리셋
            PlayerPrefs.Save();
        }
        else
        {
            // 새 게임이라면 기본 초기화 진행
            currentDay = 1;
            isTodayMissionComplete = false;

            // UI 및 이벤트 초기화
            RefreshGameStat(true); // ★ 새 게임은 이벤트 발생
        }
    }

    // ★ 상태를 화면에 적용하는 함수
    void RefreshGameStat(bool triggerEvents = false)
    {
        UpdateTaskUI();
        UpdateDayUI();

        // ★ 새 게임이거나 다음 날로 넘어갈 때만 이벤트 발생
        if (triggerEvents)
        {
            CheckForNewDayEvents();
        }

        if (weatherManager != null)
            weatherManager.SetWeather(currentDay);
    }

    // =========================================================
    // ★ [저장 기능] 오브젝트 상태 포함하여 저장
    // =========================================================
    public void SaveGameData()
    {
        if (playerTransform == null)
        {
            Debug.LogError("GameManager에 플레이어가 연결되지 않았습니다!");
            return;
        }

        try
        {
            // ★ 각 오브젝트의 고장 상태 수집
            bool waterBroken = waterPurifierObject != null && waterPurifierObject.isBroken;
            bool foodBroken = foodDeviceObject != null && foodDeviceObject.isBroken;
            bool wallBroken = wallObject != null && wallObject.isBroken;
            bool pipeBroken = pipeObject != null && pipeObject.isBroken;
            bool generatorBroken = generatorObject != null && generatorObject.isBroken;
            bool telescopeBroken = telescopeObject != null && telescopeObject.isBroken;
            bool communicateBroken = communicateObject != null && communicateObject.isBroken;
            bool lantonBroken = lantonObject != null && lantonObject.isBroken;

            // 1. 현재 상태를 데이터 봉투에 담기 (오브젝트 상태 포함)
            SaveData data = new SaveData(
                currentDay, isTodayMissionComplete, playerTransform.position,
                waterBroken, foodBroken, wallBroken, pipeBroken,
                generatorBroken, telescopeBroken, communicateBroken, lantonBroken
            );

            // 2. JSON 변환 및 파일 쓰기
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);

            Debug.Log($"저장 완료! (Day {currentDay}, 위치: {playerTransform.position})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
        }
    }

    // =========================================================
    // ★ [불러오기 기능] 오브젝트 상태 복원 포함
    // =========================================================
    public void LoadGameData()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("세이브 파일이 없어 새 게임을 시작합니다.");
            currentDay = 1;
            isTodayMissionComplete = false;
            RefreshGameStat(true); // ★ 새 게임처럼 시작
            return;
        }

        try
        {
            isLoadingGame = true; // ★ 로드 시작

            // 1. 파일 읽기
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // 2. 유효성 검사
            if (data == null)
            {
                Debug.LogError("세이브 데이터 파싱 실패!");
                StartNewGame();
                return;
            }

            // 3. 데이터 적용
            currentDay = data.day;
            isTodayMissionComplete = data.isMissionComplete;

            // 4. 플레이어 위치 이동 (CharacterController 호환)
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

            // ★ 5. 각 오브젝트의 상태 복원
            RestoreObjectStates(data);

            Debug.Log($"로드 완료! Day {currentDay}로 이동합니다. (위치: {data.playerPos})");

            // 6. 화면 갱신 (이벤트는 발생시키지 않음)
            RefreshGameStat(false); // ★ 로드 시에는 이벤트 발생 안 함

            isLoadingGame = false; // ★ 로드 완료
        }
        catch (System.Exception e)
        {
            Debug.LogError($"세이브 파일 로드 실패: {e.Message}");
            Debug.Log("새 게임으로 시작합니다.");
            StartNewGame();
        }
    }

    // =========================================================
    // ★ 오브젝트 상태 복원 메서드
    // =========================================================
    private void RestoreObjectStates(SaveData data)
    {
        // 각 오브젝트의 고장 상태 복원
        // 저장된 상태가 "고장남(true)"이면 고장 메서드 호출

        if (data.waterPurifierBroken && waterPurifierObject != null)
            waterPurifierObject.BreakPurifier();

        if (data.foodDeviceBroken && foodDeviceObject != null)
            foodDeviceObject.BreakDevice();

        if (data.wallBroken && wallObject != null)
            wallObject.BreakWall();

        if (data.pipeBroken && pipeObject != null)
            pipeObject.BreakPipe();

        if (data.generatorBroken && generatorObject != null)
            generatorObject.BreakGenerator();

        if (data.telescopeBroken && telescopeObject != null)
            telescopeObject.BreakTelescope();

        if (data.communicateBroken && communicateObject != null)
            communicateObject.BreakCommunicate();

        if (data.lantonBroken && lantonObject != null)
            lantonObject.BreakLanton();

        Debug.Log("오브젝트 상태 복원 완료");
    }

    // ★ 새 게임 시작 헬퍼 함수
    private void StartNewGame()
    {
        currentDay = 1;
        isTodayMissionComplete = false;
        isLoadingGame = false;
        RefreshGameStat(true);
    }

    // =========================================================
    // ★ 다음 날로 이동 (날짜 변경 후 저장)
    // =========================================================
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
                // 1. 날짜를 먼저 올리고
                currentDay++;

                // 2. 미션 상태 초기화
                isTodayMissionComplete = false;

                // 3. 화면 갱신 (이벤트 발생 등)
                RefreshGameStat(true);

                // 4. ★ 다음 날 아침으로 저장
                SaveGameData();
            });
        }
        else
        {
            // UIManager가 없을 때의 경우도 똑같이 순서 변경
            currentDay++;
            isTodayMissionComplete = false;
            RefreshGameStat(true);

            // ★ 여기로 이동
            SaveGameData();
        }
    }

    // =========================================================
    // ★ 날짜별 이벤트 발생 (고장 처리)
    // =========================================================
    private void CheckForNewDayEvents()
    {
        // ★ 각 오브젝트의 고장 메서드 호출
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

    // =========================================================
    // ★ 장치 수리 완료 시 호출
    // =========================================================
    public void OnDeviceFixed(string deviceName)
    {
        if (isTodayMissionComplete) return;

        bool missionMatch = false;

        // 날짜별 미션 매칭
        if ((currentDay == 1 || currentDay == 8) && deviceName == "WaterPurifier")
            missionMatch = true;
        else if ((currentDay == 2 || currentDay == 9) && deviceName == "FoodDevice")
            missionMatch = true;
        else if ((currentDay == 3 || currentDay == 10) && deviceName == "Wall")
            missionMatch = true;
        else if ((currentDay == 4 || currentDay == 11) && deviceName == "Pipe")
            missionMatch = true;
        else if ((currentDay == 5 || currentDay == 12) && deviceName == "Generator")
            missionMatch = true;
        else if ((currentDay == 6 || currentDay == 13) && deviceName == "Telescope")
            missionMatch = true;
        else if ((currentDay == 7 || currentDay == 14) && deviceName == "Communicate")
            missionMatch = true;
        else if (currentDay == 15 && deviceName == "Lanton")
            missionMatch = true;

        if (missionMatch)
        {
            isTodayMissionComplete = true;
            UpdateTaskUI();

            // ★ 미션 완료 시에도 자동 저장
            SaveGameData();
        }
    }

    // =========================================================
    // ★ UI 업데이트
    // =========================================================
    private void UpdateTaskUI()
    {
        if (taskTextUI == null) return;
        int missionIndex = currentDay - 1;

        // 미션이 없는 경우
        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count ||
            string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]))
        {
            taskTextUI.text = "모든 미션 완료!";
            return;
        }

        // 오늘 미션 완료한 경우
        if (isTodayMissionComplete)
        {
            taskTextUI.text = "오늘의 임무 완료!\n(침낭에서 자서 다음 날로)";
            return;
        }

        // 미션 텍스트 색상 처리
        string rawText = missionTextsByDay[missionIndex];
        string coloredText = rawText.Replace("목표:", "<color=#D5C790>목표:</color><color=#FFFFFF>");
        taskTextUI.text = coloredText;
    }

    private void UpdateDayUI()
    {
        if (dayTextUI != null)
            dayTextUI.text = "Day " + currentDay;
    }

    // =========================================================
    // ★ Public 접근자
    // =========================================================
    public int GetCurrentDay() => currentDay;

    public bool IsTodayMissionComplete()
    {
        int missionIndex = currentDay - 1;

        // 이미 완료 표시된 경우
        if (isTodayMissionComplete) return true;

        // 미션 범위 밖인 경우
        if (missionIndex < 0 || missionIndex >= missionTextsByDay.Count)
            return true;

        // 미션 텍스트가 비어있는 경우
        return string.IsNullOrWhiteSpace(missionTextsByDay[missionIndex]);
    }

    // =========================================================
    // ★ 수동 저장 기능 (선택사항 - 메뉴 버튼에 연결 가능)
    // =========================================================
    public void ManualSave()
    {
        SaveGameData();
        if (UIManager.Instance != null)
            UIManager.Instance.ShowNotification("게임이 저장되었습니다!");
        else
            Debug.Log("수동 저장 완료!");
    }
}
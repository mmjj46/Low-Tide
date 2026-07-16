using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

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

    [Header("Day & Night Settings")]
    public Material daySkybox;
    public Material nightSkybox;
    public Light directionalLight; // 태양빛

    [Header("Save Settings")]
    public Transform playerTransform;
    private string savePath;

    private int currentDay = 1;
    private bool isTodayMissionComplete = false;

    [Header("--- Animation & Scene Transition ---")]
    public Animator blinkAnimator;
    public AudioClip blinkSound;
    public float blinkWaitTime = 2.0f;

    [Tooltip("전환할 이름 입력 씬의 정확한 파일 이름을 적어주세요.")]
    public string nameInputSceneName = "nameinput";

    [Header("--- Dev Test (Editor Only) ---")]
    [Tooltip("체크하면 기존 세이브를 무시하고 설정한 날짜로 강제 시작합니다.")]
    public bool forceDebugDay = false;
    [Range(1, 15)]
    public int debugStartDay = 1;

    private static bool hasJumpedToNameEntry = false;

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
        string flagPath = Application.persistentDataPath + "/newgame.flag";

        // 1. 새 게임 플래그 체크 (최우선)
        if (File.Exists(flagPath))
        {
            if (File.Exists(flagPath)) File.Delete(flagPath);
            if (File.Exists(savePath)) File.Delete(savePath);
            StartNewGame();
            yield break;
        }

        // 2. 미니게임 복귀 체크
        string miniGameTarget = PlayerPrefs.GetString("MiniGameTarget", "");
        int miniGameSuccess = PlayerPrefs.GetInt("MiniGameSuccess", 0);

        if (!string.IsNullOrEmpty(miniGameTarget) && miniGameSuccess == 1)
        {
            LoadGameData();
            yield return null;
            yield return null;

            // 미니게임에서 복귀한 직후 통신 완료 상태 로그 찍기
            Debug.Log($"[GameManager] 미니게임 복귀 직후 CommunicationCompleted 상태: {PlayerPrefs.GetInt("CommunicationCompleted", 0)}");

            ProcessMiniGameReturn(miniGameTarget);
            PlayerPrefs.SetString("MiniGameTarget", "");
            PlayerPrefs.SetInt("MiniGameSuccess", 0);
            PlayerPrefs.Save();
            yield break;
        }

        // 3. 강제 테스트 모드 체크 (에디터 전용)
#if UNITY_EDITOR
        if (forceDebugDay)
        {
            Debug.Log($"[GameManager - DEV MODE] 세이브 무시! Day {debugStartDay} 강제 테스트 모드 실행");
            
            StartNewGame();

            if (debugStartDay == 8 && !hasJumpedToNameEntry)
            {
                hasJumpedToNameEntry = true; 
                
                if (blinkAnimator != null) blinkAnimator.SetTrigger("DoBlink");
                if (blinkSound != null && SoundManager.Instance != null) SoundManager.Instance.PlaySFX(blinkSound);
                
                yield return new WaitForSeconds(blinkWaitTime);
                SceneManager.LoadScene(nameInputSceneName); 
                yield break;
            }
            else
            {
                // ★ 안전장치: 이름 입력 씬을 거쳐서 다시 GameScene으로 돌아왔을 때, 
                // 다른 스크립트에 의해 통신 플래그가 1로 변했다면 여기서 강제로 0으로 초기화합니다.
                if (debugStartDay == 8 && hasJumpedToNameEntry)
                {
                    Debug.Log("[GameManager] 이름 입력 후 복귀 확인. 통신 상태를 0으로 강제 초기화합니다.");
                    PlayerPrefs.SetInt("CommunicationCompleted", 0);
                    PlayerPrefs.Save();
                }

                RefreshGameStat(true);
                yield break;
            }
        }
#endif

        // 4. 다음 날 취침 후 기상 체크
        if (PlayerPrefs.GetInt("NextDayPending", 0) == 1)
        {
            LoadGameData();
            yield return null;
            currentDay++;
            isTodayMissionComplete = false;

            PlayerPrefs.SetInt("DiaryCompleted", 0);
            PlayerPrefs.SetInt("CommunicationCompleted", 0);
            PlayerPrefs.Save();

            RefreshGameStat(true);
            SaveGameData();
            PlayerPrefs.SetInt("NextDayPending", 0);
            PlayerPrefs.Save();

            if (blinkAnimator != null)
            {
                blinkAnimator.SetTrigger("DoBlink");
                if (blinkSound != null && SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(blinkSound);
                }
            }

            if (currentDay == 8)
            {
                yield return new WaitForSeconds(blinkWaitTime);
                SceneManager.LoadScene(nameInputSceneName);
            }

            yield break;
        }

        // 5. 일반적인 기존 데이터 로드
        if (File.Exists(savePath))
        {
            LoadGameData();
        }
        else
        {
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
            case "Lanton": lantonObject?.BreakLanton(); break;
        }
    }

    void RefreshGameStat(bool triggerEvents = false)
    {
        UpdateTaskUI();
        UpdateDayUI();
        if (triggerEvents) CheckForNewDayEvents();
        if (weatherManager != null) weatherManager.SetWeather(currentDay);

        if (isTodayMissionComplete) ChangeToNight();
        else ChangeToDay();
    }

    public void SaveGameData()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
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
            StartNewGame();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            currentDay = data.day;
            isTodayMissionComplete = data.isMissionComplete;

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
        catch (System.Exception)
        {
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
        // ★ 추가: 에디터에서 바로 시작하거나 새 게임 플래그를 받았을 때도 단서 기록을 모두 날립니다!
        PlayerPrefs.DeleteAll();

#if UNITY_EDITOR
        if (forceDebugDay) currentDay = debugStartDay;
        else currentDay = 1; 
#else
        currentDay = 1;
#endif

        isTodayMissionComplete = false;

        // 데이터가 다 지워졌으니 기상 필수 플래그만 다시 세팅합니다.
        PlayerPrefs.SetInt("DiaryCompleted", 0);
        PlayerPrefs.SetInt("CommunicationCompleted", 0);
        PlayerPrefs.Save(); // 저장 확정

        RefreshGameStat(true);
        SaveGameData();
    }

    private void CheckForNewDayEvents()
    {
        if (currentDay == 1 || currentDay == 8) waterPurifierObject?.BreakPurifier();
        if (currentDay == 2 || currentDay == 9) foodDeviceObject?.BreakDevice();
        if (currentDay == 3 || currentDay == 10) wallObject?.BreakWall();
        if (currentDay == 4 || currentDay == 11) pipeObject?.BreakPipe();
        if (currentDay == 5 || 12 == currentDay) generatorObject?.BreakGenerator();
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
            ChangeToNight();
            UpdateTaskUI();
            SaveGameData();

            if (currentDay >= 7)
            {
                UIManager.Instance?.ShowNotification("오늘의 주요 수리를 완료했다.\n이제 통신기를 사용해보자.");
            }
            else
            {
                UIManager.Instance?.ShowNotification("오늘의 주요 수리를 완료했다.\n일기를 쓰고 쉴 수 있다.");
            }
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
            if (currentDay < 7)
            {
                if (PlayerPrefs.GetInt("DiaryCompleted", 0) == 1) taskTextUI.text = "<color=#FFFFFF>목표: 취침</color>";
                else taskTextUI.text = "<color=#FFFFFF>목표: 일기 작성</color>";
            }
            else
            {
                // 👇 추가된 추적 로그: 목표UI를 그릴 때 실제 저장된 통신 판정 값을 찍어봅니다.
                int commState = PlayerPrefs.GetInt("CommunicationCompleted", 0);
                Debug.Log($"[UpdateTaskUI] 현재 Day {currentDay} / 통신 완료 상태(PlayerPrefs): {commState}");

                if (commState == 1) taskTextUI.text = "<color=#FFFFFF>목표: 취침</color>";
                else taskTextUI.text = "<color=#FFFFFF>목표: 통신</color>";
            }
            return;
        }

        taskTextUI.text = $"<color=#FFFFFF>{missionTextsByDay[missionIndex]}</color>";
    }

    private void UpdateDayUI()
    {
        if (dayTextUI != null) dayTextUI.text = $"<color=#FFFFFF>DAY {currentDay}</color>";
    }

    public int GetCurrentDay() => currentDay;
    public bool IsTodayMissionComplete() => isTodayMissionComplete;

    public void ChangeToNight()
    {
        if (nightSkybox != null) RenderSettings.skybox = nightSkybox;
        if (directionalLight != null) directionalLight.intensity = 0.2f;
        DynamicGI.UpdateEnvironment();
        if (weatherManager != null) weatherManager.SetNightMode(true);
    }

    public void ChangeToDay()
    {
        if (daySkybox != null) RenderSettings.skybox = daySkybox;
        if (directionalLight != null) directionalLight.intensity = 1.0f;
        DynamicGI.UpdateEnvironment();
        if (weatherManager != null) weatherManager.SetNightMode(false);
    }
}
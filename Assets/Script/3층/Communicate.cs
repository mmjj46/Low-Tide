using UnityEngine;
using UnityEngine.SceneManagement;

public class Communicate : MonoBehaviour, IInteractable
{
    public bool isBroken = true;

    public string miniGameSceneName = "3LineConnecting";
    public string communicationSceneName = "CommunicationScene";

    private string myTargetName = "Communicate";
    private string saveKey = "Communicate_IsFixed";
    private GameManager gameManager;

    // 대화 완료 여부를 추적할 GameManager의 핵심 키와 일치시킵니다.
    private const string COMM_COMPLETED_KEY = "CommunicationCompleted";

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            isBroken = false;
            Debug.Log("Communicate: 이 장치는 이미 수리된 상태입니다.");
        }

        if (gameManager == null)
        {
            Debug.LogError("Communicate: GameManager를 찾을 수 없습니다!");
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7) && isBroken)
        {
            Debug.Log("Communicate: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        Debug.Log($"Communicate: Interact() 호출됨. 현재 isBroken = {isBroken}");

        int currentDay = (gameManager != null) ? gameManager.GetCurrentDay() : PlayerPrefs.GetInt("CurrentDay", 1);

        // 수리가 완료된 상태라면 통신 프로세스 진행
        if (!isBroken)
        {
            // ★ 수정: GameManager가 관리하고 초기화해주는 COMM_COMPLETED_KEY("CommunicationCompleted")를 검사합니다.
            if (PlayerPrefs.GetInt(COMM_COMPLETED_KEY, 0) == 1)
            {
                UIManager.Instance.ShowNotification("이미 통신을 완료했다.");
                return;
            }


            if (gameManager != null)
            {
                PlayerPrefs.SetInt("CurrentDay", currentDay);
            }

            // ★ 수정: 통신 씬으로 넘어가기 전, GameManager와 동기화되도록 완료 플래그를 1로 세웁니다.
            PlayerPrefs.SetInt(COMM_COMPLETED_KEY, 1);

            // (선택사항) 혹시 다른 곳에서 날짜별 기록이 필요할 수 있으니 기존 일자별 키도 같이 저장해둡니다.
            string commDayKey = "CommunicateDone_Day_" + currentDay;
            PlayerPrefs.SetInt(commDayKey, 1);

            PlayerPrefs.Save();

            Debug.Log($"Communicate: Day {currentDay} 통신 시작.");
            SceneManager.LoadScene(communicationSceneName);
        }
        else // 고장난 상태
        {
            if (currentDay != 7 && currentDay != 14)
            {
                UIManager.Instance.ShowNotification("고장난 통신기이다. 특정 날짜에 수리할 수 있을 것 같다.");
            }
            else
            {
                UIManager.Instance.ShowNotification("고장난 통신기이다. 고치면 누군가와 대화할 수 있을지도 모른다.");
                TryRepair();
            }
        }
    }

    public string GetInteractText()
    {
        return isBroken ? "조사: 통신기 수리" : "조사: 통신기 사용";
    }

    public void BreakCommunicate()
    {
        isBroken = true;
        PlayerPrefs.SetInt(saveKey, 0);
        PlayerPrefs.Save();
        Debug.Log("Communicate: 통신기 강제 고장 발생!");
    }

    public void TryRepair()
    {
        if (!isBroken) return;
        if (gameManager == null) return;

        int currentDay = gameManager.GetCurrentDay();

        if (currentDay != 7 && currentDay != 14)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        gameManager.SaveGameData();

        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(miniGameSceneName);
    }

    public void ForceFixFromMiniGame()
    {
        Debug.Log("Communicate: 미니게임 성공 데이터 기록");

        isBroken = false;
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.SetInt("IsTodayMissionComplete", 1);
        PlayerPrefs.Save();

        UIManager.Instance.ShowNotification("통신기를 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
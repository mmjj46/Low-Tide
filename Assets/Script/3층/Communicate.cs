using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class Communicate : MonoBehaviour, IInteractable
{
    // true = 고장(먹통), false = 정상(통신 가능)
    // 초기값 true: 시작부터 고장난 상태
    public bool isBroken = true;

    // ★ 연결할 미니게임 씬 이름 (기본값 "Random")
    // (만약 통신 관련 미니게임 씬이 따로 있다면 인스펙터에서 이름을 변경하세요!)
    public string miniGameSceneName = "LineConnecting";

    private string myTargetName = "Communicate"; // ★ GameManager가 식별할 이름
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Communicate: GameManager를 찾을 수 없습니다!");
        }

        // ★ CheckMiniGameReturn 제거 (GameManager가 처리)
    }

#if UNITY_EDITOR
    void Update()
    {
        // [테스트] 숫자 7키로 강제 수리 시도
        if (Input.GetKeyDown(KeyCode.Alpha7) && isBroken)
        {
            Debug.Log("Communicate: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    // F키 (상호작용)
    public void Interact()
    {
        Debug.Log($"Communicate: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // 정상 작동
        {
            UIManager.Instance.ShowNotification("정상 작동하는 통신기이다. 이제 아무 말이나 해 보자.");
        }
        else // 고장남
        {
            UIManager.Instance.ShowNotification("고장난 통신기이다. 고치면 누군가와 대화할 수 있을지도 모른다.");
            // 수리 시도 (미니게임 이동)
            TryRepair();
        }
    }

    // GameManager 등에서 고장낼 때 호출
    public void BreakCommunicate()
    {
        isBroken = true;
        Debug.Log("Communicate: 통신기 고장 발생!");
    }

    // 수리 시도 -> 미니게임 씬으로 이동
    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("Communicate: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // ★ 7일차 또는 14일차가 아니라면, 수리를 거부
        if (currentDay != 7 && currentDay != 14)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Communicate: 7일차 또는 14일차가 아니므로 수리 거부");
            return;
        }

        Debug.Log($"Communicate: Day {currentDay} - 미니게임 이동");

        // 1. 현재 게임 상태 저장
        gameManager.SaveGameData();

        // 2. "나 Communicate 고치러 간다"라고 메모 남기기
        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        // 3. 미니게임 씬 로드
        SceneManager.LoadScene(miniGameSceneName);
    }

    /// <summary>
    /// ★ GameManager에서 미니게임 복귀 시 호출하는 메서드
    /// </summary>
    public void ForceFixFromMiniGame()
    {
        Debug.Log("Communicate: 미니게임 성공 - 강제 수리");

        isBroken = false; // 수리 완료

        // 1. 알림 메시지
        UIManager.Instance.ShowNotification("통신기를 수리했다.");

        // 2. 게임 매니저에 보고
        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
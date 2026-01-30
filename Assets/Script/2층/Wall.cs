using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

// 역할: Day 3, Day 10 미션 오브젝트.
public class Wall : MonoBehaviour, IInteractable
{
    public bool isBroken = false; // true = 균열(고장), false = 튼튼함(정상)

    // ★ 연결할 미니게임 씬 이름
    // (만약 지우기 게임이 준비 안 됐다면 인스펙터에서 "Random"으로 바꾸세요!)
    public string miniGameSceneName = "StainErasing";

    private string myTargetName = "Wall"; // ★ GameManager가 식별할 이름
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Wall: GameManager를 찾을 수 없습니다!");
        }

        // ★ CheckMiniGameReturn 제거 (GameManager가 처리)
    }

#if UNITY_EDITOR
    void Update()
    {
        // [테스트] 숫자 3키로 강제 수리 시도
        if (Input.GetKeyDown(KeyCode.Alpha3) && isBroken)
        {
            Debug.Log("Wall: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    // F키 (상호작용)
    public void Interact()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("주먹으로 벽을 두드렸다. 이제 튼튼하다.");
        }
        else
        {
            // 균열이 있으면 수리 시도 (미니게임 이동)
            TryRepair();
        }
    }

    public void BreakWall()
    {
        isBroken = true; // 균열 발생
        Debug.Log("Wall: 균열 발생!");
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
            Debug.LogError("Wall: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // ★ 3일차 또는 10일차가 아니라면, 수리를 거부
        if (currentDay != 3 && currentDay != 10)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Wall: 3일차 또는 10일차가 아니므로 수리 거부");
            return;
        }

        Debug.Log($"Wall: Day {currentDay} - 미니게임 이동");

        // 1. 현재 게임 상태 저장
        gameManager.SaveGameData();

        // 2. "나 Wall 고치러 간다"라고 메모 남기기
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
        Debug.Log("Wall: 미니게임 성공 - 강제 수리");

        isBroken = false;

        // 1. 알림 메시지
        UIManager.Instance.ShowNotification("벽의 균열을 메웠다.");

        // 2. 게임 매니저에 보고
        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class Generator : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "Random"; // 연결할 미니게임 씬 이름

    private string myTargetName = "Generator"; // ★ GameManager가 식별할 이름
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("Generator: GameManager를 찾을 수 없습니다!");
        }

        // ★ CheckMiniGameReturn 제거! (GameManager가 처리함)
    }

#if UNITY_EDITOR
    void Update()
    {
        // [테스트] 숫자 5키로 강제 수리 시도
        if (Input.GetKeyDown(KeyCode.Alpha5) && isBroken)
        {
            Debug.Log("Generator: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        Debug.Log($"Generator: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("발전기가 웅웅거리며 작동 중이다.");
        }
        else
        {
            // 고장났으면 수리 시도 (미니게임 이동)
            TryRepair();
        }
    }

    public void BreakGenerator()
    {
        isBroken = true;
        Debug.Log("Generator: 고장 발생!");
    }

    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("Generator: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // ★ 5일차 또는 12일차가 아니라면, 수리를 거부
        if (currentDay != 5 && currentDay != 12)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Generator: 5일차 또는 12일차가 아니므로 수리 거부");
            return;
        }

        Debug.Log($"Generator: Day {currentDay} - 미니게임 이동");

        // 1. 현재 게임 상태 저장
        gameManager.SaveGameData();

        // 2. "나 Generator 고치러 간다"라고 메모 남기기
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
        Debug.Log("Generator: 미니게임 성공 - 강제 수리");

        isBroken = false;
        UIManager.Instance.ShowNotification("발전기를 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
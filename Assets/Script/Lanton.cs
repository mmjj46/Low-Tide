using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class Lanton : MonoBehaviour, IInteractable
{
    // true = 고장(불 꺼짐), false = 정상(불 켜짐)
    // 초기값 true: 시작은 항상 고장 상태로 시작
    public bool isBroken = true;

    // ★ 연결할 미니게임 씬 이름 (기본값 "Random")
    // 만약 전구 켜기 게임 등을 따로 만드셨다면 인스펙터에서 이름을 변경하세요!
    public string miniGameSceneName = "Random";

    private string myTargetName = "Lanton"; // ★ GameManager가 식별할 이름
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Lanton: GameManager를 찾을 수 없습니다!");
        }

        // ★ CheckMiniGameReturn 제거 (GameManager가 처리)
    }

#if UNITY_EDITOR
    void Update()
    {
        // [테스트] 엔터 키로 강제 수리 시도
        if (Input.GetKeyDown(KeyCode.Return) && isBroken)
        {
            Debug.Log("Lanton: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    // F키 (상호작용)
    public void Interact()
    {
        Debug.Log($"Lanton: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // 정상 작동 중
        {
            UIManager.Instance.ShowNotification("세상이 밝아졌다. 당신의 마음도 조금이나마 밝아진다.");
        }
        else // 고장남
        {
            UIManager.Instance.ShowNotification("신기하게 생긴 등명기이다. 고치면 불을 밝힐 수 있다.");
            // 수리 시도
            TryRepair();
        }
    }

    // GameManager 등에서 고장낼 때 호출
    public void BreakLanton()
    {
        isBroken = true;
        Debug.Log("Lanton: 등명기 고장 발생!");
    }

    // 수리 시도 -> 미니게임 씬으로 이동
    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 불이 켜져 있다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("Lanton: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // ★ 15일차가 아니라면, 수리를 거부
        if (currentDay != 15)
        {
            UIManager.Instance.ShowNotification("지금은 불을 밝힐 때가 아니다.");
            Debug.Log("Lanton: 15일차가 아니므로 수리 거부");
            return;
        }

        Debug.Log($"Lanton: Day {currentDay} - 미니게임 이동");

        // 1. 현재 게임 상태 저장
        gameManager.SaveGameData();

        // 2. "나 Lanton 고치러 간다"라고 메모 남기기
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
        Debug.Log("Lanton: 미니게임 성공 - 강제 수리");

        isBroken = false; // 수리 완료

        // 1. 알림 메시지 (감동적인 멘트 유지)
        UIManager.Instance.ShowNotification("세상이 밝아졌다. 당신의 마음도 조금이나마 밝아진다.");

        // 2. 게임 매니저에 보고
        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
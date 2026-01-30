using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class Pipe : MonoBehaviour, IInteractable
{
    public bool isBroken = false; // true = 연결 안 됨(고장), false = 연결됨(정상)

    // ★ 파이프 미니게임 씬 이름 (Dot 게임으로 추정되어 "Dot"으로 설정함)
    // 만약 씬 이름이 다르다면 유니티 인스펙터에서 수정해주세요!
    public string miniGameSceneName = "Pipe_2";

    private string myTargetName = "Pipe"; // ★ GameManager가 식별할 이름
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Pipe: GameManager를 찾을 수 없습니다!");
        }

        // ★ CheckMiniGameReturn 제거 (GameManager가 처리)
    }

#if UNITY_EDITOR
    void Update()
    {
        // [테스트] 숫자 4키로 강제 연결 시도
        if (Input.GetKeyDown(KeyCode.Alpha4) && isBroken)
        {
            Debug.Log("Pipe: [테스트] 강제 연결 시도");
            TryConnect();
        }
    }
#endif

    // F키 (상호작용)
    public void Interact()
    {
        Debug.Log($"Pipe: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("깨끗한 물이 흐르고 있다.");
        }
        else
        {
            // 고장났으면 연결 시도 (미니게임 이동)
            TryConnect();
        }
    }

    public void BreakPipe()
    {
        isBroken = true;
        Debug.Log("Pipe: 파이프 연결 해제!");
    }

    // 연결 시도 -> 미니게임 씬으로 이동
    public void TryConnect()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 연결되었다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("Pipe: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // ★ 4일차 또는 11일차가 아니라면, 연결 거부
        if (currentDay != 4 && currentDay != 11)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 연결할 때가 아니다.");
            Debug.Log("Pipe: 4일차 또는 11일차가 아니므로 연결 거부");
            return;
        }

        Debug.Log($"Pipe: Day {currentDay} - 미니게임 이동");

        // 1. 현재 게임 상태 저장
        gameManager.SaveGameData();

        // 2. "나 Pipe 고치러 간다"라고 메모 남기기
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
        Debug.Log("Pipe: 미니게임 성공 - 강제 연결");

        isBroken = false; // 연결 완료
        UIManager.Instance.ShowNotification("파이프를 연결했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
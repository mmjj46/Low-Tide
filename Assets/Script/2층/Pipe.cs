using UnityEngine;

public class Pipe : MonoBehaviour, IInteractable
{
    // ★ GameManager가 참조할 고장 상태 (public으로 변경)
    public bool isBroken = false; // true = 연결 안 됨(고장), false = 연결됨(정상)

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Pipe: GameManager를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // [임시] 숫자 4키로 연결 (미니게임 완성 전까지)
        if (Input.GetKeyDown(KeyCode.Alpha4) && isBroken)
        {
            Debug.Log("Pipe: 4키 감지 -> TryConnect() 호출");
            TryConnect();
        }
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"Pipe: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // ★ 연결되어 있으면 (정상)
        {
            UIManager.Instance.ShowNotification("깨끗한 물이 흐르고 있다.");
        }
        else // ★ 연결 안 되어 있으면 (고장)
        {
            UIManager.Instance.ShowNotification("물을 쓰려면 파이프를 연결해야 한다.");
            // (나중에 미니게임 시작 코드 추가)
        }
    }

    // 연결 함수 (미니게임이나 다른 방식으로 호출)
    public void TryConnect()
    {
        Debug.Log("Pipe: TryConnect() 호출됨");

        if (!isBroken) // ★ 이미 연결되어 있으면
        {
            UIManager.Instance.ShowNotification("이미 연결되었다.");
            return;
        }

        Connect();
    }

    public void Connect()
    {
        Debug.Log($"Pipe: Connect() 호출됨. 현재 날짜 = {gameManager.GetCurrentDay()}");

        // [추가된 안전장치]
        // 오늘이 4일차 또는 11일차가 아니라면, 연결을 거부하고 함수 종료
        int currentDay = gameManager.GetCurrentDay();
        if (currentDay != 4 && currentDay != 11)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 연결할 때가 아니다.");
            Debug.Log("Pipe: 4일차 또는 11일차가 아니므로 연결 거부");
            return;
        }

        if (!isBroken) return; // ★ 중복 방지 (이미 연결되어 있으면 리턴)

        isBroken = false; // ★ 연결 완료 (false = 정상)
        Debug.Log("Pipe: 연결 완료! GameManager에 보고합니다.");

        UIManager.Instance.ShowNotification("파이프를 연결했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("Pipe");
        }
    }

    // ★ GameManager가 호출할 함수 (4일차, 11일차에 고장)
    public void BreakPipe()
    {
        isBroken = true; // ★ true = 연결 해제됨 (고장)
        Debug.Log("Pipe.cs: 파이프 연결 해제! (isBroken = true)");
    }
}
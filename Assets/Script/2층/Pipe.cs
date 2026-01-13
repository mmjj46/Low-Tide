using UnityEngine;

public class Pipe : MonoBehaviour, IInteractable
{
    private bool isConnected = false;
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
        if (Input.GetKeyDown(KeyCode.Alpha4) && !isConnected)
        {
            Debug.Log("Pipe: 4키 감지 -> TryConnect() 호출");
            TryConnect();
        }
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"Pipe: Interact() 호출됨. isConnected = {isConnected}");

        if (isConnected)
        {
            UIManager.Instance.ShowNotification("깨끗한 물이 흐르고 있다.");
        }
        else
        {
            UIManager.Instance.ShowNotification("물을 쓰려면 파이프를 연결해야 한다.");
            // (나중에 미니게임 시작 코드 추가)
        }
    }

    // 연결 함수 (미니게임이나 다른 방식으로 호출)
    public void TryConnect()
    {
        Debug.Log("Pipe: TryConnect() 호출됨");

        if (isConnected)
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

        if (isConnected) return;
        isConnected = true;

        Debug.Log("Pipe: 연결 완료! GameManager에 보고합니다.");
        UIManager.Instance.ShowNotification("파이프를 연결했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("Pipe");
        }
    }

    // [GameManager가 호출할 함수]
    // Day 4, Day 11이 되면 GameManager가 호출해서 강제로 '연결 해제' 시킴
    public void BreakPipe()
    {
        isConnected = false;
        Debug.Log("Pipe.cs: 파이프 연결 해제! (isConnected = false)");
    }
}
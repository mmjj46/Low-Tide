using UnityEngine;

public class Generator : MonoBehaviour, IInteractable
{
    // true: 작동함, false: 고장
    private bool isWorking = true;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Generator: GameManager를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // [임시] 숫자 5키로 수리 (미니게임 완성 전까지)
        if (Input.GetKeyDown(KeyCode.Alpha5) && !isWorking)
        {
            Debug.Log("Generator: 5키 감지 -> TryRepair() 호출");
            TryRepair();
        }
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"Generator: Interact() 호출됨. isWorking = {isWorking}");

        if (isWorking)
        {
            UIManager.Instance.ShowNotification("발전기가 웅웅거리며 작동 중이다.");
        }
        else
        {
            UIManager.Instance.ShowNotification("발전기가 고장났다. 수리해야 한다.");
            // (나중에 미니게임 시작 코드 추가)
        }
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        Debug.Log("Generator: TryRepair() 호출됨");

        if (isWorking)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        Fix();
    }

    public void Fix()
    {
        Debug.Log($"Generator: Fix() 호출됨. 현재 날짜 = {gameManager.GetCurrentDay()}");

        // [추가된 안전장치]
        // 오늘이 5일차 또는 12일차가 아니라면, 수리를 거부하고 함수 종료
        int currentDay = gameManager.GetCurrentDay();
        if (currentDay != 5 && currentDay != 12)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Generator: 5일차 또는 12일차가 아니므로 수리 거부");
            return;
        }

        if (isWorking) return; // 중복 방지
        isWorking = true;

        Debug.Log("Generator: 수리 완료! GameManager에 보고합니다.");
        UIManager.Instance.ShowNotification("발전기를 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("Generator");
        }
    }

    // [GameManager가 호출할 함수]
    // Day 5, Day 12가 되면 GameManager가 호출해서 강제로 '고장' 냄
    public void BreakGenerator()
    {
        isWorking = false;
        Debug.Log("Generator.cs: 발전기 고장 발생! (isWorking = false)");
    }
}
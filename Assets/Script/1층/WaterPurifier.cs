using UnityEngine;

public class WaterPurifier : MonoBehaviour, IInteractable
{
    // ★ GameManager가 참조할 고장 상태 (public으로 변경)
    public bool isBroken = false; // true = 고장남, false = 정상

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("WaterPurifier: GameManager를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // [임시] 숫자 1키로 수리 (미니게임 완성 전까지)
        if (Input.GetKeyDown(KeyCode.Alpha1) && isBroken)
        {
            TryRepair();
        }
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        if (!isBroken) // ★ 고장나지 않았으면 정상 작동
        {
            UIManager.Instance.ShowNotification("당장의 목마름을 해결했다.");
        }
        else // ★ 고장났으면 작동 안 함
        {
            UIManager.Instance.ShowNotification("마실 물을 정수하는 장치이다. 작동하지 않는다.");
        }
    }

    // ★ GameManager가 호출할 함수 (1일차, 8일차에 고장)
    public void BreakPurifier()
    {
        isBroken = true; // ★ true = 고장남
        Debug.Log("WaterPurifier.cs: 정수기 고장 발생! (isBroken = true)");
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        if (!isBroken) // ★ 이미 수리되어 있으면
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        Fix();
    }

    public void Fix()
    {
        int currentDay = gameManager.GetCurrentDay();

        // [수정] 1일차 또는 8일차가 아니라면, 수리를 거부
        if (currentDay != 1 && currentDay != 8)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("WaterPurifier: 1일차 또는 8일차가 아니므로 수리 거부");
            return;
        }

        if (!isBroken) return; // ★ 이미 수리되어 있으면 리턴

        isBroken = false; // ★ 수리 완료 (false = 정상)
        UIManager.Instance.ShowNotification("정수기를 수리했다.");

        if (gameManager != null)
            gameManager.OnDeviceFixed("WaterPurifier");
    }
}
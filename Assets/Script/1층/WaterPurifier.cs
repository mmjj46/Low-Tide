using UnityEngine;

public class WaterPurifier : MonoBehaviour, IInteractable
{
    // false로 시작 (1일차 미션을 위해)
    private bool isFixed = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        // [임시] 숫자 1키로 수리 (미니게임 완성 전까지)
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isFixed)
        {
            TryRepair();
        }
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        if (isFixed)
        {
            UIManager.Instance.ShowNotification("당장의 목마름을 해결했다.");
        }
        else
        {
            UIManager.Instance.ShowNotification("마실 물을 정수하는 장치이다. 작동하지 않는다.");
        }
    }

    // [추가] 8일차가 되면 GameManager가 호출할 함수
    public void BreakPurifier()
    {
        isFixed = false;
        Debug.Log("WaterPurifier.cs: 정수기 고장 발생! (isFixed = false)");
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        if (isFixed)
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

        if (isFixed) return;
        isFixed = true;

        UIManager.Instance.ShowNotification("정수기를 수리했다.");
        if (gameManager != null) gameManager.OnDeviceFixed("WaterPurifier");
    }
}
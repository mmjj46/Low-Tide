using UnityEngine;

public class FoodDevice : MonoBehaviour, IInteractable
{
    // false로 시작 (2일차 미션을 위해)
    private bool isFixed = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("FoodDevice: GameManager를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // [임시] 숫자 2키로 수리 (미니게임 완성 전까지)
        if (Input.GetKeyDown(KeyCode.Alpha2) && !isFixed)
        {
            Debug.Log("FoodDevice: 2키 감지 -> TryRepair() 호출");
            TryRepair();
        }
    }

    // [추가] 9일차가 되면 GameManager가 호출할 함수
    public void BreakDevice()
    {
        isFixed = false;
        Debug.Log("FoodDevice.cs: 식량 장치 고장 발생! (isFixed = false)");
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"FoodDevice: Interact() 호출됨. isFixed = {isFixed}");

        if (isFixed)
        {
            UIManager.Instance.ShowNotification("당장의 허기를 해결했다.");
        }
        else
        {
            UIManager.Instance.ShowNotification("식량을 만드는 장치이다. 작동하지 않는다.");
        }
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        Debug.Log("FoodDevice: TryRepair() 호출됨");

        if (isFixed)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        Fix();
    }

    public void Fix()
    {
        Debug.Log($"FoodDevice: Fix() 호출됨. 현재 날짜 = {gameManager.GetCurrentDay()}");

        int currentDay = gameManager.GetCurrentDay();

        // [수정] 2일차 또는 9일차가 아니라면, 수리를 거부
        if (currentDay != 2 && currentDay != 9)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("FoodDevice: 2일차 또는 9일차가 아니므로 수리 거부");
            return;
        }

        if (isFixed) return;
        isFixed = true;

        Debug.Log("FoodDevice: 수리 완료! GameManager에 보고합니다.");
        UIManager.Instance.ShowNotification("식량 제조 장치를 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("FoodDevice");
        }
    }
}
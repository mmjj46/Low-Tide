using UnityEngine;

public class FoodDevice : MonoBehaviour, IInteractable
{
    // ★ GameManager가 참조할 고장 상태 (public으로 변경)
    public bool isBroken = false; // true = 고장남, false = 정상

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
        if (Input.GetKeyDown(KeyCode.Alpha2) && isBroken)
        {
            Debug.Log("FoodDevice: 2키 감지 -> TryRepair() 호출");
            TryRepair();
        }
    }

    // ★ GameManager가 호출할 함수 (2일차, 9일차에 고장)
    public void BreakDevice()
    {
        isBroken = true; // ★ true = 고장남
        Debug.Log("FoodDevice.cs: 식량 장치 고장 발생! (isBroken = true)");
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"FoodDevice: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // ★ 고장나지 않았으면 정상 작동
        {
            UIManager.Instance.ShowNotification("당장의 허기를 해결했다.");
        }
        else // ★ 고장났으면 작동 안 함
        {
            UIManager.Instance.ShowNotification("식량을 만드는 장치이다. 작동하지 않는다.");
        }
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        Debug.Log("FoodDevice: TryRepair() 호출됨");

        if (!isBroken) // ★ 이미 수리되어 있으면
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

        if (!isBroken) return; // ★ 이미 수리되어 있으면 리턴

        isBroken = false; // ★ 수리 완료 (false = 정상)
        Debug.Log("FoodDevice: 수리 완료! GameManager에 보고합니다.");

        UIManager.Instance.ShowNotification("식량 제조 장치를 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("FoodDevice");
        }
    }
}
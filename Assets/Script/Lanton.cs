using UnityEngine;

public class Lanton : MonoBehaviour, IInteractable
{
    // ★ GameManager가 참조할 고장 상태 (public으로 변경)
    public bool isBroken = true; // true = 고장남, false = 정상 (초기값 true: 시작은 항상 고장)

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Lanton: GameManager를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // 엔터 키로 수리
        if (Input.GetKeyDown(KeyCode.Return) && isBroken)
        {
            Debug.Log("Lanton: 엔터키 감지 -> TryRepair() 호출");
            TryRepair();
        }
    }

    public void Interact()
    {
        Debug.Log($"Lanton: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // ★ 고장나지 않았으면 정상 작동
        {
            UIManager.Instance.ShowNotification("세상이 밝아졌다. 당신의 마음도 조금이나마 밝아진다.");
        }
        else // ★ 고장났으면 수리 필요
        {
            UIManager.Instance.ShowNotification("신기하게 생긴 등명기이다. 고치면 불을 밝힐 수 있다.");
        }
    }

    public void TryRepair()
    {
        Debug.Log("Lanton: TryRepair() 호출됨");

        if (!isBroken) // ★ 이미 수리되어 있으면
        {
            UIManager.Instance.ShowNotification("이미 불이 켜져 있다.");
            return;
        }

        Fix();
    }

    public void Fix()
    {
        int currentDay = gameManager.GetCurrentDay();
        Debug.Log($"Lanton: Fix() 호출됨. 현재 날짜 = {currentDay}");

        // 15일차가 아니면 수리 불가
        if (currentDay != 15)
        {
            UIManager.Instance.ShowNotification("지금은 불을 밝힐 때가 아니다.");
            Debug.Log("Lanton: 15일차가 아니므로 수리 불가");
            return;
        }

        // 수리 실행
        if (isBroken) // ★ 고장난 상태라면
        {
            isBroken = false; // ★ 수리 완료 (false = 정상)
            UIManager.Instance.ShowNotification("세상이 밝아졌다. 당신의 마음도 조금이나마 밝아진다.");
            Debug.Log("Lanton: 수리 완료!");

            if (gameManager != null)
                gameManager.OnDeviceFixed("Lanton");
        }
    }

    // ★ GameManager가 호출할 함수 (15일차에 고장)
    public void BreakLanton()
    {
        isBroken = true; // ★ true = 고장남
        Debug.Log("Lanton.cs: 등명기 고장 발생! (isBroken = true)");
    }
}
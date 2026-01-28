using UnityEngine;

public class Communicate : MonoBehaviour, IInteractable
{
    // ★ GameManager가 참조할 고장 상태 (public으로 변경)
    public bool isBroken = true; // true = 고장남, false = 정상 (초기값 true: 처음부터 고장)

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Communicate: GameManager를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // [임시] 숫자 7키로 수리 (미니게임 완성 전까지)
        if (Input.GetKeyDown(KeyCode.Alpha7) && isBroken)
        {
            Debug.Log("Communicate: 7키 감지 -> TryRepair() 호출");
            TryRepair();
        }
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"Communicate: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // ★ 고장나지 않았으면 정상 작동
        {
            UIManager.Instance.ShowNotification("정상 작동하는 통신기이다. 이제 아무 말이나 해 보자.");
        }
        else // ★ 고장났으면 수리 필요
        {
            UIManager.Instance.ShowNotification("고장난 통신기이다. 고치면 누군가와 대화할 수 있을지도 모른다.");
            // (나중에 미니게임 시작 코드 추가)
        }
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        Debug.Log("Communicate: TryRepair() 호출됨");

        if (!isBroken) // ★ 이미 수리되어 있으면
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        Fix();
    }

    public void Fix()
    {
        Debug.Log($"Communicate: Fix() 호출됨. 현재 날짜 = {gameManager.GetCurrentDay()}");

        // [추가된 안전장치]
        // 오늘이 7일차 또는 14일차가 아니라면, 수리를 거부하고 함수 종료
        int currentDay = gameManager.GetCurrentDay();
        if (currentDay != 7 && currentDay != 14)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Communicate: 7일차 또는 14일차가 아니므로 수리 거부");
            return;
        }

        if (!isBroken) return; // ★ 중복 방지 (이미 수리되어 있으면 리턴)

        isBroken = false; // ★ 수리 완료 (false = 정상)
        Debug.Log("Communicate: 수리 완료! GameManager에 보고합니다.");

        UIManager.Instance.ShowNotification("통신기를 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("Communicate");
        }
    }

    // ★ GameManager가 호출할 함수 (7일차, 14일차에 고장)
    public void BreakCommunicate()
    {
        isBroken = true; // ★ true = 고장남
        Debug.Log("Communicate.cs: 통신기 고장 발생! (isBroken = true)");
    }
}
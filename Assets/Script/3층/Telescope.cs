using UnityEngine;

public class Telescope : MonoBehaviour, IInteractable
{
    // ★ GameManager가 참조할 고장 상태 (public으로 변경)
    public bool isBroken = false; // true = 고장남, false = 정상

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Telescope: GameManager를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // [임시] 숫자 6키로 수리 (미니게임 완성 전까지)
        if (Input.GetKeyDown(KeyCode.Alpha6) && isBroken)
        {
            Debug.Log("Telescope: 6키 감지 -> TryRepair() 호출");
            TryRepair();
        }
    }

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"Telescope: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // ★ 고장나지 않았으면 정상 작동
        {
            // 0.8(80%) 확률: 바다/하늘만 보임
            // 0.2(20%) 확률: 새가 보임
            float r = Random.value;
            Debug.Log($"랜덤 값: {r}");

            if (r <= 0.8f)
            {
                UIManager.Instance.ShowNotification("아무리 들여다봐도 푸른 바다와 하늘만 보일 뿐이다.");
            }
            else
            {
                UIManager.Instance.ShowNotification("수평선 위를 날아가는 새가 보인다. 근처에 육지가 있는 걸까?");
            }
        }
        else // ★ 고장났으면 작동 안 함
        {
            UIManager.Instance.ShowNotification("쓴 지 오래된 망원경이다. 렌즈 너머가 잘 보이지 않는다.");
            // (나중에 미니게임 시작 코드 추가)
        }
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        Debug.Log("Telescope: TryRepair() 호출됨");

        if (!isBroken) // ★ 이미 수리되어 있으면
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        Fix();
    }

    public void Fix()
    {
        Debug.Log($"Telescope: Fix() 호출됨. 현재 날짜 = {gameManager.GetCurrentDay()}");

        // [추가된 안전장치]
        // 오늘이 6일차 또는 13일차가 아니라면, 수리를 거부하고 함수 종료
        int currentDay = gameManager.GetCurrentDay();
        if (currentDay != 6 && currentDay != 13)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Telescope: 6일차 또는 13일차가 아니므로 수리 거부");
            return;
        }

        if (!isBroken) return; // ★ 중복 방지 (이미 수리되어 있으면 리턴)

        isBroken = false; // ★ 수리 완료 (false = 정상)
        Debug.Log("Telescope: 수리 완료! GameManager에 보고합니다.");

        UIManager.Instance.ShowNotification("망원경을 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("Telescope");
        }
    }

    // ★ GameManager가 호출할 함수 (6일차, 13일차에 고장)
    public void BreakTelescope()
    {
        isBroken = true; // ★ true = 고장남
        Debug.Log("Telescope.cs: 망원경 고장 발생! (isBroken = true)"); // ★ 로그 메시지도 수정 (발전기 -> 망원경)
    }
}
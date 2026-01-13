using UnityEngine;

// 역할: Day 3, Day 10 미션 오브젝트. 'Enter' 키로 테스트 완료 가능
public class Wall : MonoBehaviour, IInteractable
{
    // true: 튼튼함, false: 균열 (고장)
    private bool isSturdy = true;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // F키 (상호작용)
    public void Interact()
    {
        if (isSturdy)
        {
            // (수리 후 피드백)
            UIManager.Instance.ShowNotification("주먹으로 벽을 두드렸다. 이제 튼튼하다.");
        }
        else
        {
            // (수리 전 피드백)
            UIManager.Instance.ShowNotification("주먹으로 벽을 두드렸다. 자갈이 후드득 쏟아진다.");
            // (나중에 미니게임 시작 코드 추가)
        }
    }

    // [임시] 숫자 3키로 수리 (미니게임 완성 전까지)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && !isSturdy)
        {
            TryRepair();
        }
    }

    // 수리 함수 (미니게임이나 다른 방식으로 호출)
    public void TryRepair()
    {
        if (isSturdy)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        Fix();
    }

    // [GameManager가 호출할 함수 1]
    // Day 3, Day 10이 되면 GameManager가 호출해서 강제로 '고장' 냄
    public void BreakWall()
    {
        isSturdy = false;
        Debug.Log("Wall.cs: 균열 발생! (isSturdy = false)");
    }

    // [GameManager가 호출할 함수 2]
    // 플레이어가 수리 미니게임을 완료했을 때 (지금은 테스트 키로) 호출
    public void Fix()
    {
        // [추가된 안전장치]
        // 오늘이 3일차 또는 10일차가 아니라면, 수리를 거부하고 함수 종료
        int currentDay = gameManager.GetCurrentDay();
        if (currentDay != 3 && currentDay != 10)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        if (isSturdy) return; // 중복 방지
        isSturdy = true;

        // 1. 알림 센터에 "수리 완료" 메시지 요청
        UIManager.Instance.ShowNotification("벽의 균열을 메웠다.");

        // 2. 게임 매니저에 "미션 완료" 보고
        if (gameManager != null)
        {
            // "Wall" 이라는 이름으로 보고
            gameManager.OnDeviceFixed("Wall");
        }
    }
}
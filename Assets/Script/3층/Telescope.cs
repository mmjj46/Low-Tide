using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class Telescope : MonoBehaviour, IInteractable
{
    public bool isBroken = false; // true = 고장(렌즈 더러움), false = 정상

    // ★ 연결할 미니게임 씬 이름 
    // (망원경은 렌즈를 닦는 것이 어울려서 StainErasing으로 설정했습니다. 
    // 원하시면 인스펙터에서 "Random"으로 바꾸셔도 됩니다!)
    public string miniGameSceneName = "StainErasing";

    private string myTargetName = "Telescope"; // ★ GameManager가 식별할 이름
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Telescope: GameManager를 찾을 수 없습니다!");
        }

        // ★ CheckMiniGameReturn 제거 (GameManager가 처리)
    }

#if UNITY_EDITOR
    void Update()
    {
        // [테스트] 숫자 6키로 강제 수리 시도
        if (Input.GetKeyDown(KeyCode.Alpha6) && isBroken)
        {
            Debug.Log("Telescope: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"Telescope: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // ★ 고장나지 않았으면 (정상 작동)
        {
            // 기존의 랜덤 관측 로직 유지
            float r = Random.value;

            if (r <= 0.8f)
            {
                UIManager.Instance.ShowNotification("아무리 들여다봐도 푸른 바다와 하늘만 보일 뿐이다.");
            }
            else
            {
                UIManager.Instance.ShowNotification("수평선 위를 날아가는 새가 보인다. 근처에 육지가 있는 걸까?");
            }
        }
        else // ★ 고장났으면 (미니게임 이동)
        {
            // 쓴 지 오래된 망원경이다... -> 수리 시도
            TryRepair();
        }
    }

    public void BreakTelescope()
    {
        isBroken = true; // 고장남
        Debug.Log("Telescope: 망원경 고장 발생!");
    }

    // 수리 시도 -> 미니게임 씬으로 이동
    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 깨끗하다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("Telescope: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // ★ 6일차 또는 13일차가 아니라면, 수리를 거부
        if (currentDay != 6 && currentDay != 13)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Telescope: 6일차 또는 13일차가 아니므로 수리 거부");
            return;
        }

        Debug.Log($"Telescope: Day {currentDay} - 미니게임 이동");

        // 1. 현재 게임 상태 저장
        gameManager.SaveGameData();

        // 2. "나 Telescope 고치러 간다"라고 메모 남기기
        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        // 3. 미니게임 씬 로드
        SceneManager.LoadScene(miniGameSceneName);
    }

    /// <summary>
    /// ★ GameManager에서 미니게임 복귀 시 호출하는 메서드
    /// </summary>
    public void ForceFixFromMiniGame()
    {
        Debug.Log("Telescope: 미니게임 성공 - 강제 수리");

        isBroken = false; // 수리 완료

        // 1. 알림 메시지
        UIManager.Instance.ShowNotification("망원경 렌즈를 깨끗이 닦았다.");

        // 2. 게임 매니저에 보고
        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
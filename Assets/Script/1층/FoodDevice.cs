using UnityEngine;
using UnityEngine.SceneManagement;

public class FoodDevice : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "Random";

    private string myTargetName = "FoodDevice";
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("FoodDevice: GameManager를 찾을 수 없습니다!");
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) && isBroken)
        {
            Debug.Log("FoodDevice: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("당장의 허기를 해결했다.");
        }
        else
        {
            TryRepair();
        }
    }

    public void BreakDevice()
    {
        isBroken = true;
        Debug.Log("FoodDevice: 고장 발생!");
    }

    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("FoodDevice: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        if (currentDay != 2 && currentDay != 9)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        Debug.Log($"FoodDevice: Day {currentDay} - 미니게임 이동");

        gameManager.SaveGameData();

        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(miniGameSceneName);
    }

    /// <summary>
    /// ★ GameManager에서 미니게임 복귀 시 호출하는 메서드
    /// </summary>
    public void ForceFixFromMiniGame()
    {
        Debug.Log("FoodDevice: 미니게임 성공 - 강제 수리");

        isBroken = false;
        UIManager.Instance.ShowNotification("식량 제조 장치를 수리했다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }
}
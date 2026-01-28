using UnityEngine;
using UnityEngine.SceneManagement;

public class WaterPurifier : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "Random";

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // 미니게임 성공하고 돌아왔는지 확인
        if (PlayerPrefs.GetString("MiniGameTarget") == "WaterPurifier" &&
            PlayerPrefs.GetInt("MiniGameSuccess") == 1)
        {
            // ★ 먼저 초기화 (중복 방지)
            PlayerPrefs.SetInt("MiniGameSuccess", 0);
            PlayerPrefs.SetString("MiniGameTarget", "");
            PlayerPrefs.Save();

            Fix();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && isBroken)
        {
            TryRepair();
        }
    }

    public void Interact()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("당장의 목마름을 해결했다.");
        }
        else
        {
            TryRepair();
        }
    }

    public void BreakPurifier()
    {
        isBroken = true;
        Debug.Log("WaterPurifier: 고장남!");
    }

    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();
        if (currentDay != 1 && currentDay != 8)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        Debug.Log("미니게임으로 이동합니다...");

        gameManager.SaveGameData();

        // ★ 타겟 설정하고 씬 전환
        PlayerPrefs.SetString("MiniGameTarget", "WaterPurifier");
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(miniGameSceneName);
    }

    public void Fix()
    {
        if (!isBroken) return; // 이미 수리됨

        isBroken = false;

        UIManager.Instance.ShowNotification("정수기 수리에 성공했다!");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed("WaterPurifier");
            gameManager.SaveGameData();
        }
    }
}
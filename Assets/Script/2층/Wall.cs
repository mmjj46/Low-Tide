using UnityEngine;
using UnityEngine.SceneManagement;

public class Wall : MonoBehaviour, IInteractable
{
    public bool isBroken = false;

    public string miniGameSceneName = "StainErasing";

    private string myTargetName = "Wall";
    private GameManager gameManager;

    // ★ [변경] GameObject 대신 MeshRenderer를 직접 담을 변수
    public MeshRenderer brokenGraphicRenderer;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Wall: GameManager를 찾을 수 없습니다!");
        }

        UpdateGraphicState();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && isBroken)
        {
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("주먹으로 벽을 두드렸다. 이제 튼튼하다.");
        }
        else
        {
            TryRepair();
        }
    }

    public void BreakWall()
    {
        isBroken = true;
        UpdateGraphicState(); // ★ 깨진 그래픽 켜기 (Renderer.enabled = true)
        Debug.Log("Wall: 균열 발생!");
    }

    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        if (gameManager == null) return;

        int currentDay = gameManager.GetCurrentDay();

        if (currentDay != 3 && currentDay != 10)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        Debug.Log($"Wall: Day {currentDay} - 미니게임 이동");

        gameManager.SaveGameData();
        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(miniGameSceneName);
    }

    public void ForceFixFromMiniGame()
    {
        Debug.Log("Wall: 미니게임 성공 - 강제 수리");

        isBroken = false;
        UpdateGraphicState(); // ★ 깨진 그래픽 끄기 (Renderer.enabled = false)

        UIManager.Instance.ShowNotification("벽의 균열을 메웠다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }

    // ★ Renderer를 껐다 켜는 함수
    private void UpdateGraphicState()
    {
        if (brokenGraphicRenderer != null)
        {
            // isBroken이 true면 -> 렌더러를 켠다 (보임)
            // isBroken이 false면 -> 렌더러를 끈다 (안 보임)
            brokenGraphicRenderer.enabled = isBroken;
        }
    }
}
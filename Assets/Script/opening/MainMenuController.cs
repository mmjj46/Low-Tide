using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "GameScene"; // 이동할 씬 이름 정확히!
    public GameObject settingsPanel;

    // [이어하기 버튼 연결]
    public void OnContinue()
    {
        // 1 = 이어하기 신호
        PlayerPrefs.SetInt("IsLoadGame", 1);
        PlayerPrefs.Save(); // 확실하게 저장
        SceneManager.LoadScene(gameSceneName);
    }

    // [새 게임 버튼 연결]
    public void OnNewGame()
    {
        // 0 = 새 게임 신호
        PlayerPrefs.SetInt("IsLoadGame", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
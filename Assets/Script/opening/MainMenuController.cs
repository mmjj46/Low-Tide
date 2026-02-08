using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "GameScene"; // 이동할 씬 이름
    public GameObject settingsPanel;

    /// <summary>
    /// 이어하기 버튼
    /// </summary>
    public void OnContinue()
    {
        Debug.Log("[MainMenu] 이어하기 버튼 클릭");

        // 이어하기 플래그 설정
        PlayerPrefs.SetInt("IsLoadGame", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// 새 게임 버튼
    /// </summary>
    public void OnNewGame()
    {
        Debug.LogWarning("★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★");
        Debug.LogWarning("★★★ NEW GAME 버튼 클릭! ★★★");
        Debug.LogWarning("★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★");

        // 1. 세이브 파일 삭제
        string savePath = Application.persistentDataPath + "/savefile.json";
        Debug.Log($"[MainMenu] 세이브 파일 경로: {savePath}");
        Debug.Log($"[MainMenu] 삭제 전 파일 존재: {File.Exists(savePath)}");

        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                Debug.Log("[MainMenu] ✓ 세이브 파일 삭제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainMenu] ✗ 세이브 파일 삭제 실패: {e.Message}");
            }
        }

        Debug.Log($"[MainMenu] 삭제 후 파일 존재: {File.Exists(savePath)}");

        // 2. 새 게임 플래그 파일 생성
        string flagPath = Application.persistentDataPath + "/newgame.flag";
        try
        {
            File.WriteAllText(flagPath, "1");
            Debug.LogWarning($"[MainMenu] ✓✓✓ 새 게임 플래그 생성 완료! ✓✓✓");
            Debug.Log($"[MainMenu] 플래그 파일 존재: {File.Exists(flagPath)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MainMenu] ✗ 플래그 파일 생성 실패: {e.Message}");
        }

        // 3. PlayerPrefs 게임 데이터 삭제
        PlayerPrefs.DeleteKey("IsLoadGame");
        PlayerPrefs.DeleteKey("NextDayPending");
        PlayerPrefs.DeleteKey("MiniGameTarget");
        PlayerPrefs.DeleteKey("MiniGameSuccess");
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] PlayerPrefs 초기화 완료");

        // 4. 게임 씬 로드
        Debug.LogWarning($"[MainMenu] ★★★ 게임 씬 로드: {gameSceneName} ★★★");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void OnQuit()
    {
        Debug.Log("[MainMenu] 게임 종료");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
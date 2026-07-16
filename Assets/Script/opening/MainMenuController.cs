using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "GameScene"; // 이동할 씬 이름
    public GameObject settingsPanel;

    public void OnContinue()
    {
        Debug.Log("[MainMenu] 이어하기 버튼 클릭");
        PlayerPrefs.SetInt("IsLoadGame", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnNewGame()
    {
        Debug.LogWarning("★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★");
        Debug.LogWarning("★★★ NEW GAME 버튼 클릭! ★★★");
        Debug.LogWarning("★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★");

        // 1. 세이브 파일 삭제
        string savePath = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(savePath))
        {
            try { File.Delete(savePath); Debug.Log("[MainMenu] ✓ 세이브 파일 삭제 완료"); }
            catch (System.Exception e) { Debug.LogError($"[MainMenu] ✗ 세이브 파일 삭제 실패: {e.Message}"); }
        }

        // 2. 새 게임 플래그 파일 생성
        string flagPath = Application.persistentDataPath + "/newgame.flag";
        try
        {
            File.WriteAllText(flagPath, "1");
            Debug.LogWarning($"[MainMenu] ✓✓✓ 새 게임 플래그 생성 완료! ✓✓✓");
        }
        catch (System.Exception e) { Debug.LogError($"[MainMenu] ✗ 플래그 파일 생성 실패: {e.Message}"); }

        // 3. ★★★ 핵심 수정: 과거의 모든 기억(PlayerPrefs)을 한 방에 완벽 삭제 ★★★
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] PlayerPrefs 전체 초기화 완료 (이름, 날짜, 단서 등 모두 삭제)");

        // 4. 게임 씬 로드
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
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
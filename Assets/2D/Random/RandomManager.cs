using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomManager : MonoBehaviour
{
    [Header("게임 설정")]
    [Range(0f, 100f)]
    public float changeChance = 30f;

    [Header("UI 연결")]
    public List<SpriteSwitch> allSwitches = new List<SpriteSwitch>();

    public static RandomManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. 커서를 보이게 설정
        Cursor.visible = true;

        // 2. 커서 잠금을 해제 (화면 밖으로 나갈 수 있게/자유롭게 움직이게)
        Cursor.lockState = CursorLockMode.None;
    }

    public void CheckGameClear()
    {
        if (allSwitches == null || allSwitches.Count == 0) return;

        // 모두 켜졌는지 확인
        foreach (var sw in allSwitches)
        {
            if (sw == null || !sw.isSpriteChanged)
                return; // 하나라도 안 켜졌으면 종료
        }

        // 모두 켜졌으면 클리어!
        Debug.Log("게임 클리어! 0.5초 뒤 복귀합니다.");
        StartCoroutine(ReturnToMainGame());
    }

    IEnumerator ReturnToMainGame()
    {
        yield return new WaitForSeconds(0.5f);

        // ★ 성공 표시하고 메인 씬으로 복귀
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameScene");
    }
}
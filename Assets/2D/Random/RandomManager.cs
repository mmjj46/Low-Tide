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

    [Header("사운드")]
    public AudioClip clearSound; // ★ 1. 효과음 연결
    private AudioSource audioSource;

    public static RandomManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // ★ 디버그: 어떤 장치를 수리하러 왔는지 확인
        string target = PlayerPrefs.GetString("MiniGameTarget", "Unknown");
        Debug.Log($"[RandomManager] 미니게임 시작 - 타겟: {target}");
    }

    public void CheckGameClear()
    {
        if (allSwitches == null || allSwitches.Count == 0) return;

        foreach (var sw in allSwitches)
        {
            if (sw == null || !sw.isSpriteChanged)
                return;
        }

        Debug.Log("게임 클리어! 0.5초 뒤 복귀합니다.");
        StartCoroutine(ReturnToMainGame());
    }

    IEnumerator ReturnToMainGame()
    {
        if (clearSound != null) audioSource.PlayOneShot(clearSound);
        yield return new WaitForSeconds(0.5f);

        // ★ [수정] 성공 표시만 하고 복귀 (Target은 삭제하지 않음)
        // Device 스크립트에서 확인 후 초기화할 것
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        string target = PlayerPrefs.GetString("MiniGameTarget", "Unknown");
        Debug.Log($"[RandomManager] 미니게임 성공! 타겟: {target}");

        SceneManager.LoadScene("GameScene");
    }
}
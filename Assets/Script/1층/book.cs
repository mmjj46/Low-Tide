using UnityEngine;
using UnityEngine.SceneManagement;

public class book : MonoBehaviour, IInteractable
{
    [Header("씬 설정")]
    public string diarySceneName = "DiaryScene";

    [Header("Audio Settings")]
    public AudioClip openSound;
    private GameManager gameManager;
    private AudioSource audioSource;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Interact()
    {
        if (gameManager == null)
        {
            Debug.LogError("[book] GameManager를 찾을 수 없습니다!");
            return;
        }

        // ★ 미션 완료 여부와 관계없이 무조건 일기장 씬으로 이동합니다.
        // 미션 미완료 시 단서 탭 표시 / 완료 시 일기 탭 표시는 DiaryTabManager가 처리합니다.
        GoToDiaryScene();
    }

    public string GetInteractText()
    {
        return "조사: 일기 작성";
    }

    void GoToDiaryScene()
    {
        int currentDay = gameManager.GetCurrentDay();
        Debug.Log($"[book] Day {currentDay} 일기장 씬으로 이동: {diarySceneName}");

        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        gameManager.SaveGameData();

        // 1. 일기장 씬으로 현재 날짜 정보 넘겨주기
        PlayerPrefs.SetInt("CurrentDay", currentDay);

        // ★ 2. 핵심 추가: 오늘 미션 완료 여부를 확실하게 일기장 씬에 넘겨줍니다!
        // IsTodayMissionComplete()가 true면 1, false면 0을 저장합니다.
        int missionStatus = gameManager.IsTodayMissionComplete() ? 1 : 0;
        PlayerPrefs.SetInt("IsTodayMissionComplete", missionStatus);

        PlayerPrefs.Save();

        SceneManager.LoadScene(diarySceneName);
    }
}
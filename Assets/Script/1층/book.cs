using UnityEngine;
using UnityEngine.SceneManagement;

public class book : MonoBehaviour, IInteractable
{
    [Header("Day별 씬 이름 설정")]
    public string[] diarySceneNames = new string[15]; // Day 1~15에 해당하는 씬 이름

    [Header("Audio Settings")]
    public AudioClip openSound; // 책 펼치는 소리

    private GameManager gameManager;
    private AudioSource audioSource;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // F키 상호작용
    public void Interact()
    {
        // 게임 매니저 확인
        if (gameManager == null)
        {
            Debug.LogError("[book] GameManager를 찾을 수 없습니다!");
            return;
        }

        // 오늘의 미션 완료 여부 확인
        if (!gameManager.IsTodayMissionComplete())
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification("아직 할 일이 남았다. 일기를 쓸 수 없다.");
            }
            return;
        }

        // 미션 완료 시 해당 Day의 일기장 씬으로 이동
        GoToDiaryScene();
    }

    void GoToDiaryScene()
    {
        int currentDay = gameManager.GetCurrentDay();
        int sceneIndex = currentDay - 1; // 배열은 0부터 시작

        // 유효성 검사
        if (sceneIndex < 0 || sceneIndex >= diarySceneNames.Length)
        {
            Debug.LogError($"[book] Day {currentDay}에 해당하는 씬 인덱스가 범위를 벗어났습니다!");
            return;
        }

        string targetScene = diarySceneNames[sceneIndex];

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError($"[book] Day {currentDay}에 해당하는 씬 이름이 설정되지 않았습니다!");
            return;
        }

        Debug.Log($"[book] Day {currentDay} 일기장 씬으로 이동: {targetScene}");

        // 소리 재생
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // 현재 날짜 저장
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.Save();

        // 해당 Day의 씬으로 이동
        SceneManager.LoadScene(targetScene);
    }
}
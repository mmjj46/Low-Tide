using UnityEngine;
using UnityEngine.SceneManagement;

public class book : MonoBehaviour, IInteractable
{
    [Header("씬 설정")]
    public string diarySceneName = "DiaryScene"; // ★ 단일 일기장 씬

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
                UIManager.Instance.ShowNotification("아직 할 일을 남았다. 일기를 쓸 수 없다.");
            }
            return;
        }

        // 미션 완료 시 일기장 씬으로 이동
        GoToDiaryScene();
    }

    void GoToDiaryScene()
    {
        int currentDay = gameManager.GetCurrentDay();

        Debug.Log($"[book] Day {currentDay} 일기장 씬으로 이동: {diarySceneName}");

        // 소리 재생
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // ★★★ [핵심 수정] 씬 이동 직전에 현재 위치(책상 앞)를 저장합니다! ★★★
        // 이걸 해야 일기를 쓰고 돌아왔을 때 미니게임 장소가 아니라 여기 서 있습니다.
        gameManager.SaveGameData();

        // 현재 날짜 저장 (DiaryDialogue에서 이 값을 읽어서 해당 텍스트 파일 로드)
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.Save();

        // 일기장 씬으로 이동
        SceneManager.LoadScene(diarySceneName);
    }
}
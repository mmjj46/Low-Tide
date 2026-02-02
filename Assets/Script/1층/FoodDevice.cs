using UnityEngine;
using UnityEngine.SceneManagement;

public class FoodDevice : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "Random"; // ★ 인스펙터에서 실제 미니게임 씬 이름(예: StainErasing) 확인!

    private string myTargetName = "FoodDevice";
    private GameManager gameManager;

    [Header("사운드 이펙트")]
    public AudioClip workingSound; // 정상 작동 소리 (기계 위잉~, 음식 나오는 소리)
    public AudioClip brokenSound;  // 고장난 소리 (경고음 삐-삐-, 혹은 기계가 덜덜거리는 소리)
    private AudioSource audioSource;

    // ★ Start 대신 Awake 사용 (GameManager보다 먼저 준비)
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("FoodDevice: GameManager를 찾을 수 없습니다!");
        }

        // 오디오 소스 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2) && isBroken)
        {
            Debug.Log("FoodDevice: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        if (!isBroken)
        {
            // ★ 정상 상태: 맛있는 음식 나오는 소리
            PlaySound(workingSound);
            UIManager.Instance.ShowNotification("당장의 허기를 해결했다.");
        }
        else
        {
            // ★ 고장 상태: 만약 소리가 멈춰있다면 다시 켜줌
            if (!audioSource.isPlaying && brokenSound != null)
            {
                audioSource.clip = brokenSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            UIManager.Instance.ShowNotification("식량 배급 장치가 고장났다. 작동하지 않는다.");

            // 바로 수리 화면으로 진입
            TryRepair();
        }
    }

    // ★ GameManager가 아침에 호출하는 고장 함수
    public void BreakDevice()
    {
        isBroken = true;
        Debug.Log("FoodDevice: 고장 발생!");

        // ★★★ 고장 나자마자 경고음/고장음 재생 ★★★
        if (brokenSound != null && audioSource != null)
        {
            audioSource.clip = brokenSound;
            audioSource.loop = true; // 계속 시끄럽게 하려면 true (추천: 기계 고장난 느낌)
            audioSource.Play();
        }
    }

    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 수리되었다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("FoodDevice: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // 2일차 또는 9일차가 아니면 수리 불가
        if (currentDay != 2 && currentDay != 9)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        Debug.Log($"FoodDevice: Day {currentDay} - 미니게임 이동");

        // 씬 이동 전 소리 끄기
        if (audioSource != null) audioSource.Stop();

        gameManager.SaveGameData();

        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(miniGameSceneName);
    }

    /// <summary>
    /// ★ GameManager에서 미니게임 복귀 시 호출하는 메서드
    /// </summary>
    public void ForceFixFromMiniGame()
    {
        Debug.Log("FoodDevice: 미니게임 성공 - 강제 수리");

        isBroken = false;

        // ★★★ 1. 시끄러운 고장 소리 끄기
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false; // 루프 해제
        }

        // ★★★ 2. 수리 완료 피드백 (정상 작동 소리)
        PlaySound(workingSound);

        UIManager.Instance.ShowNotification("식량 제조 장치를 수리했다. 맛있는 냄새가 난다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }

    // 소리 재생 헬퍼 함수
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.loop = false; // 일반 효과음은 반복하지 않음
            audioSource.PlayOneShot(clip);
        }
    }
}
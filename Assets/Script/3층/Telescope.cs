using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class Telescope : MonoBehaviour, IInteractable
{
    public bool isBroken = false; // true = 고장(렌즈 더러움/깨짐), false = 정상

    // ★ 망원경은 렌즈 닦기(StainErasing)가 어울림
    public string miniGameSceneName = "StainErasing";

    private string myTargetName = "Telescope"; // ★ GameManager가 식별할 이름
    private GameManager gameManager;

    [Header("사운드 이펙트")]
    public AudioClip workingSound; // 정상 작동 소리 (렌즈 돌아가는 소리, 발견 효과음)
    public AudioClip brokenSound;  // 고장난 소리 (유리 금가는 소리, 끽끽거리는 소리)
    private AudioSource audioSource;

    // ★ Start -> Awake로 변경 (안전성 확보)
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Telescope: GameManager를 찾을 수 없습니다!");
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
        if (Input.GetKeyDown(KeyCode.Alpha6) && isBroken)
        {
            Debug.Log("Telescope: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    // F키 (상호작용) - 평소 사용
    public void Interact()
    {
        Debug.Log($"Telescope: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken) // ★ 고장나지 않았으면 (정상 작동)
        {
            // ★ 정상 소리 재생
            PlaySound(workingSound);

            // 기존의 랜덤 관측 로직 유지
            float r = Random.value;

            if (r <= 0.8f)
            {
                UIManager.Instance.ShowNotification("아무리 들여다봐도 푸른 바다와 하늘만 보일 뿐이다.");
            }
            else
            {
                UIManager.Instance.ShowNotification("수평선 위를 날아가는 새가 보인다. 근처에 육지가 있는 걸까?");
            }
        }
        else // ★ 고장났으면 (미니게임 이동)
        {
            // ★ 고장 소리 재생 (안 나고 있다면)
            if (!audioSource.isPlaying && brokenSound != null)
            {
                audioSource.clip = brokenSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            UIManager.Instance.ShowNotification("쓴 지 오래된 망원경이다. 렌즈 너머가 잘 보이지 않는다.");

            // 수리 시도
            TryRepair();
        }
    }

    public string GetInteractText()
    {
        // 벽이 깨졌을 때와 아닐 때의 텍스트를 다르게 리턴합니다.
        if (isBroken)
        {
            return "조사: 망원경 렌즈 닦기";
        }
        else
        {
            return "조사: 망원경";
        }
    }

    // ★ GameManager가 아침에 호출하는 고장 함수
    public void BreakTelescope()
    {
        isBroken = true; // 고장남
        Debug.Log("Telescope: 망원경 고장 발생!");

        // ★★★ 고장 나자마자 소리 재생 ★★★
        if (brokenSound != null && audioSource != null)
        {
            audioSource.clip = brokenSound;
            // 망원경은 보통 '챙그랑' 하고 끝나므로 loop는 false가 어울리지만, 
            // '바람 소리' 같은 거라면 true로 하셔도 됩니다.
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    // 수리 시도 -> 미니게임 씬으로 이동
    public void TryRepair()
    {
        if (!isBroken)
        {
            UIManager.Instance.ShowNotification("이미 깨끗하다.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("Telescope: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // ★ 6일차 또는 13일차가 아니라면, 수리를 거부
        if (currentDay != 6 && currentDay != 13)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Telescope: 6일차 또는 13일차가 아니므로 수리 거부");
            return;
        }

        Debug.Log($"Telescope: Day {currentDay} - 미니게임 이동");

        // 씬 이동 전 소리 끄기
        if (audioSource != null) audioSource.Stop();

        // 1. 현재 게임 상태 저장
        gameManager.SaveGameData();

        // 2. "나 Telescope 고치러 간다"라고 메모 남기기
        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        // 3. 미니게임 씬 로드
        SceneManager.LoadScene(miniGameSceneName);
    }

    /// <summary>
    /// ★ GameManager에서 미니게임 복귀 시 호출하는 메서드
    /// </summary>
    public void ForceFixFromMiniGame()
    {
        Debug.Log("Telescope: 미니게임 성공 - 강제 수리");

        isBroken = false; // 수리 완료

        // ★★★ 1. 고장 소리 끄기
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // ★★★ 2. 수리 완료 피드백 (렌즈 닦은 뽀득 소리 or 발견 소리)
        PlaySound(workingSound);

        // 1. 알림 메시지
        UIManager.Instance.ShowNotification("망원경 렌즈를 깨끗이 닦았다.");

        // =========================================================
        // ★★★ 3. 오늘 하루 미션 완료 처리! (이 코드가 일기장을 엽니다) ★★★
        PlayerPrefs.SetInt("IsTodayMissionComplete", 1);
        PlayerPrefs.Save();
        // =========================================================

        // 2. 게임 매니저에 보고
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
            audioSource.loop = false;
            audioSource.PlayOneShot(clip);
        }
    }
}
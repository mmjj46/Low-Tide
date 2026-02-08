using UnityEngine;
using UnityEngine.SceneManagement;

public class Generator : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "PipeConnecting"; // ★ 인스펙터에서 연결할 미니게임 씬 이름 확인!

    private string myTargetName = "Generator";
    private GameManager gameManager;

    [Header("사운드 이펙트")]
    public AudioClip workingSound; // 정상 작동 소리 (웅웅~ 발전기 돌아가는 소리)
    public AudioClip brokenSound;  // 고장난 소리 (스파크 튀는 소리, 혹은 전원 꺼지는 소리)
    private AudioSource audioSource;

    // ★ Start -> Awake로 변경 (안전성 확보)
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("Generator: GameManager를 찾을 수 없습니다!");
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
        if (Input.GetKeyDown(KeyCode.Alpha5) && isBroken)
        {
            Debug.Log("Generator: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        Debug.Log($"Generator: Interact() 호출됨. isBroken = {isBroken}");

        if (!isBroken)
        {
            // ★ 정상 상태: 웅웅거리는 작동음 재생
            PlaySound(workingSound);
            UIManager.Instance.ShowNotification("발전기가 웅웅거리며 작동 중이다.");
        }
        else
        {
            // ★ 고장 상태: 소리가 꺼져있다면 다시 켬
            if (!audioSource.isPlaying && brokenSound != null)
            {
                audioSource.clip = brokenSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            // 고장났으면 수리 시도
            TryRepair();
        }
    }
    public string GetInteractText()
    {
        // 벽이 깨졌을 때와 아닐 때의 텍스트를 다르게 리턴합니다.
        if (isBroken)
        {
            return "조사: 발전기 수리";
        }
        else
        {
            return "조사: 발전기";
        }
    }

    // ★ GameManager가 아침에 호출하는 고장 함수
    public void BreakGenerator()
    {
        isBroken = true;
        Debug.Log("Generator: 고장 발생!");

        // ★★★ 고장 나자마자 소리 재생 (스파크/경고음) ★★★
        if (brokenSound != null && audioSource != null)
        {
            audioSource.clip = brokenSound;
            audioSource.loop = true; // 발전기 고장은 계속 시끄러운 게 어울림
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
            Debug.LogError("Generator: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        // 5일차 또는 12일차가 아니면 수리 불가
        if (currentDay != 5 && currentDay != 12)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            Debug.Log("Generator: 5일차 또는 12일차가 아니므로 수리 거부");
            return;
        }

        Debug.Log($"Generator: Day {currentDay} - 미니게임 이동");

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
        Debug.Log("Generator: 미니게임 성공 - 강제 수리");

        isBroken = false;

        // ★★★ 1. 고장 소리 끄기
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // ★★★ 2. 수리 완료 피드백 (재가동 소리)
        PlaySound(workingSound);

        UIManager.Instance.ShowNotification("발전기를 수리했다.");

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
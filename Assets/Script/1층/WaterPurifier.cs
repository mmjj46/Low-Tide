using UnityEngine;
using UnityEngine.SceneManagement;

public class WaterPurifier : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "Random"; // ★ 인스펙터에서 "StainErasing"인지 꼭 확인!

    private string myTargetName = "WaterPurifier";
    private GameManager gameManager;

    [Header("사운드 이펙트")]
    public AudioClip workingSound; // 정상 작동 소리 (물 따르는 소리)
    public AudioClip brokenSound;  // 고장난 소리 (물 새는 소리, 기계 웅웅거림)
    private AudioSource audioSource;

    // ★ Start 대신 Awake 사용! 
    // GameManager가 Start에서 BreakPurifier를 부를 때, 오디오가 준비되어 있어야 하기 때문입니다.
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("WaterPurifier: GameManager를 찾을 수 없습니다!");
        }

        // 오디오 소스 컴포넌트 가져오기 (없으면 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && isBroken)
        {
            Debug.Log("WaterPurifier: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        if (!isBroken)
        {
            // ★ 정상 상태: 물 따르는 소리 (한 번 재생)
            PlaySound(workingSound);
            UIManager.Instance.ShowNotification("당장의 목마름을 해결했다.");
        }
        else
        {
            // ★ 고장 상태: 만약 소리가 안 나고 있다면 다시 켜줌
            if (!audioSource.isPlaying)
            {
                audioSource.clip = brokenSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            UIManager.Instance.ShowNotification("정수기가 고장났다. 물이 나오지 않는다.");
            TryRepair();
        }
    }
    public string GetInteractText()
    {
        // 벽이 깨졌을 때와 아닐 때의 텍스트를 다르게 리턴합니다.
        if (isBroken)
        {
            return "조사: 정수기 수리";
        }
        else
        {
            return "조사: 정수기";
        }
    }
    // ★ GameManager가 아침에 호출하는 고장 함수
    public void BreakPurifier()
    {
        isBroken = true;
        Debug.Log("WaterPurifier: 고장 발생!");

        // ★★★ 핵심: 고장 나자마자 소리 재생 (물 새는 소리 등) ★★★
        if (brokenSound != null && audioSource != null)
        {
            audioSource.clip = brokenSound;
            audioSource.loop = true; // 계속 들리게 함 (물 새는 소리라면 true 추천)
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
            Debug.LogError("WaterPurifier: GameManager가 없습니다!");
            return;
        }

        int currentDay = gameManager.GetCurrentDay();

        if (currentDay != 1 && currentDay != 8)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        Debug.Log($"WaterPurifier: Day {currentDay} - 미니게임 이동");

        // 씬 이동 전에는 소리를 끄고 감 (선택 사항)
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
        Debug.Log("WaterPurifier: 미니게임 성공 - 강제 수리");

        isBroken = false;

        // ★★★ 1. 고장 소리(Loop) 끄기
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false; // 반복 끄기
        }

        // ★★★ 2. 수리 완료 피드백 (성공 소리)
        PlaySound(workingSound);

        UIManager.Instance.ShowNotification("정수기를 수리했다.");

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
            audioSource.loop = false; // 일반 소리는 반복 안 함
            audioSource.PlayOneShot(clip);
        }
    }
}
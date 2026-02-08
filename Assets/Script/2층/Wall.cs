using UnityEngine;
using UnityEngine.SceneManagement;

public class Wall : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "StainErasing"; // 미니게임 씬 이름 확인

    private string myTargetName = "Wall";
    private GameManager gameManager;

    // ★ [기존 유지] 그래픽 담당
    public MeshRenderer brokenGraphicRenderer;

    [Header("사운드 이펙트")]
    public AudioClip workingSound; // 정상일 때 (벽 두드리는 둔탁한 소리 '쿵')
    public AudioClip brokenSound;  // 고장났을 때 (바람 새는 소리 '휘잉~' or 갈라지는 소리)
    private AudioSource audioSource;

    // ★ Start -> Awake로 변경 (안전성 확보)
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("Wall: GameManager를 찾을 수 없습니다!");
        }

        // 오디오 소스 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        UpdateGraphicState();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3) && isBroken)
        {
            Debug.Log("Wall: [테스트] 강제 수리 시도");
            TryRepair();
        }
    }
#endif

    public void Interact()
    {
        if (!isBroken)
        {
            // ★ 정상 상태: 벽 두드리는 소리
            PlaySound(workingSound);
            UIManager.Instance.ShowNotification("주먹으로 벽을 두드렸다. 이제 튼튼하다.");
        }
        else
        {
            // ★ 고장 상태: 만약 소리가 멈췄다면 다시 켜줌
            if (!audioSource.isPlaying && brokenSound != null)
            {
                audioSource.clip = brokenSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            UIManager.Instance.ShowNotification("벽에 금이 갔다. 찬바람이 들어온다.");
            TryRepair();
        }
    }
    // Wall.cs의 GetInteractText 부분을 이렇게 수정해 보세요!
    public string GetInteractText()
    {
        // 벽이 깨졌을 때와 아닐 때의 텍스트를 다르게 리턴합니다.
        if (isBroken)
        {
            return "조사: 벽 수리";
        }
        else
        {
            return "조사: 벽";
        }
    }
    // ★ GameManager가 아침에 호출하는 고장 함수
    public void BreakWall()
    {
        isBroken = true;
        UpdateGraphicState(); // 깨진 그래픽 켜기
        Debug.Log("Wall: 균열 발생!");

        // ★★★ 고장 나자마자 소리 재생 (바람 새는 소리 등) ★★★
        if (brokenSound != null && audioSource != null)
        {
            audioSource.clip = brokenSound;
            audioSource.loop = true; // 바람 소리라면 계속 나게 true 추천
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

        if (gameManager == null) return;

        int currentDay = gameManager.GetCurrentDay();

        if (currentDay != 3 && currentDay != 10)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        Debug.Log($"Wall: Day {currentDay} - 미니게임 이동");

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
        Debug.Log("Wall: 미니게임 성공 - 강제 수리");

        isBroken = false;
        UpdateGraphicState(); // 깨진 그래픽 끄기

        // ★★★ 1. 바람 새는 소리 끄기
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // ★★★ 2. 수리 완료 피드백 (단단해진 소리)
        PlaySound(workingSound);

        UIManager.Instance.ShowNotification("벽의 균열을 메웠다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }

    private void UpdateGraphicState()
    {
        if (brokenGraphicRenderer != null)
        {
            brokenGraphicRenderer.enabled = isBroken;
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
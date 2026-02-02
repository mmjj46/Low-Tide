using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FoodDevice : MonoBehaviour, IInteractable
{
    public bool isBroken = false;
    public string miniGameSceneName = "Random";

    private string myTargetName = "FoodDevice";
    private GameManager gameManager;

    [Header("사운드 이펙트")]
    public AudioClip workingSound;
    public AudioClip brokenSound;
    private AudioSource audioSource;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ★ 시작할 때 확실하게 루프 꺼두기
        audioSource.loop = false;
    }

#if UNITY_EDITOR
    void Update()
    {
        // 테스트용: 이미 고장났으면 실행 안 되게 막아둠
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
            PlaySound(workingSound);
            UIManager.Instance.ShowNotification("당장의 허기를 해결했다.");
        }
        else
        {
            // 고장 났을 때 클릭해도 소리는 다시 켜지 않음 (메세지만 출력)
            UIManager.Instance.ShowNotification("식량 배급 장치가 고장났다. 작동하지 않는다.");
            TryRepair();
        }
    }

    public void BreakDevice()
    {
        // ★★★ [가장 중요한 수정] ★★★
        // 이미 고장난 상태라면(이미 소리가 울렸거나 울리는 중이라면)
        // 함수를 바로 종료해서 소리가 겹치는 것을 막습니다.
        if (isBroken) return;

        isBroken = true;
        Debug.Log("FoodDevice: 고장 발생!");

        if (brokenSound != null && audioSource != null)
        {
            StopAllCoroutines(); // 혹시 모를 중복 방지
            StartCoroutine(PlayBrokenSoundTimes(3));
        }
    }

    IEnumerator PlayBrokenSoundTimes(int count)
    {
        audioSource.loop = false;

        for (int i = 0; i < count; i++)
        {
            // 수리되었으면 즉시 중단
            if (!isBroken) yield break;

            // PlayOneShot 대신 Play 사용 (확실한 제어를 위해)
            audioSource.clip = brokenSound;
            audioSource.Play();

            // 소리 길이만큼 대기
            yield return new WaitForSeconds(brokenSound.length);
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

        if (currentDay != 2 && currentDay != 9)
        {
            UIManager.Instance.ShowNotification("지금은 이걸 수리할 때가 아니다.");
            return;
        }

        // 씬 이동 전 소리 끄기
        StopAllCoroutines();
        if (audioSource != null) audioSource.Stop();

        gameManager.SaveGameData();

        PlayerPrefs.SetString("MiniGameTarget", myTargetName);
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(miniGameSceneName);
    }

    public void ForceFixFromMiniGame()
    {
        isBroken = false;
        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        PlaySound(workingSound);
        UIManager.Instance.ShowNotification("식량 제조 장치를 수리했다. 맛있는 냄새가 난다.");

        if (gameManager != null)
        {
            gameManager.OnDeviceFixed(myTargetName);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(clip);
        }
    }
}
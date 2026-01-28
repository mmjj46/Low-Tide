using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 어디서든 SoundManager.Instance로 부를 수 있게 만드는 '싱글톤' 패턴
    public static SoundManager Instance;

    [Header("UI Sounds (2D)")]
    public AudioClip clickClip;        // 클릭 소리 넣을 곳
    public AudioClip hoverClip;        // 마우스 올릴 때 소리 넣을 곳

    // 나중에 필요한 소리가 생기면 여기에 계속 추가하면 됩니다.
    // public AudioClip successClip; 
    // public AudioClip failClip;

    private AudioSource audioSource;

    void Awake()
    {
        // SoundManager는 게임 내내 하나만 있어야 하므로 중복 제거
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바껴도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 소리를 낼 스피커(AudioSource) 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 소리 재생 함수 (외부에서 이걸 호출해서 소리 냄)
    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
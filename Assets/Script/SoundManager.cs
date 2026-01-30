using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM Settings")]
    public AudioClip mainBGM;          // ★ 배경음악 파일 넣는 곳
    [Range(0f, 1f)] public float bgmVolume = 0.5f;

    [Header("SFX Clips")]
    public AudioClip clickClip;        // 클릭 소리
    public AudioClip hoverClip;        // 마우스 올릴 때 소리

    // 스피커를 2개 둡니다 (음악용, 효과음용)
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 파괴 안 됨
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ★ 스피커 2개 자동 생성 및 설정 ★

        // 1. BGM용 스피커 만들기
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;          // 노래는 무한 반복
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        // 2. SFX용 스피커 만들기
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;         // 효과음은 한 번만 재생

        // 게임 시작하자마자 BGM 재생
        PlayBGM(mainBGM);
    }

    // ★ 배경음악 재생 함수
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // 이미 같은 노래가 나오고 있다면 다시 틀지 않음 (끊김 방지 핵심!)
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // ★ 효과음 재생 함수
    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // 볼륨 조절이 필요할 때 쓸 함수 (옵션)
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }
}
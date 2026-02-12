using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM Settings")]
    public AudioClip mainBGM;
    // bgmVolume 변수는 삭제했습니다. (이제 Audio Mixer로 조절하니까요!)

    [Header("SFX Clips")]
    public AudioClip clickClip;
    public AudioClip hoverClip;

    [Header("Audio Sources (Drag & Drop)")]
    // ★ private을 public으로 바꿔서 인스펙터에서 보이게 함
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ★ 더 이상 코드로 스피커를 만들지 않습니다 (AddComponent 삭제)
        // 대신, 인스펙터에 연결된 스피커가 있는지 확인만 합니다.

        if (bgmSource != null && mainBGM != null)
        {
            PlayBGM(mainBGM);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        // sfxSource가 연결되어 있을 때만 재생
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
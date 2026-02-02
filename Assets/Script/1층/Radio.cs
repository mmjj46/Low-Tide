using UnityEngine;

public class RadioInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip radioNoiseSound; // ★ 지직거리는 잡음 파일 (mp3/wav)
    private AudioSource audioSource;

    void Awake()
    {
        // 오디오 소스 컴포넌트 가져오기 (없으면 자동으로 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Interact()
    {
        // ★ 1. 소리 재생 (PlayOneShot: 겹쳐서 재생 가능, 반복 안 함)
        if (radioNoiseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(radioNoiseSound);
        }

        // 2. 텍스트 출력
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("다이얼을 돌려봐도 잡음만 들린다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
}
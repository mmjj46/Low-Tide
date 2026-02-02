using UnityEngine;

public class toolboxInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip toolSound; // ★ 쇠 부딪히는 소리 (Metal Clank, Rummage)
    private AudioSource audioSource;

    void Awake()
    {
        // 오디오 소스 컴포넌트 가져오기 (없으면 자동 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Interact()
    {
        // ★ 1. 소리 재생
        if (toolSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(toolSound);
        }

        // 2. 텍스트 출력
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("무거운 공구가 가득하다. 절반 정도는 용도를 모르겠다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
}
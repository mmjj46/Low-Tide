using UnityEngine;

public class WaterProofInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip interactSound; // ★ 옷 부스럭거리는 소리 or 젖은 소리
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
        if (interactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactSound);
        }

        // 2. 텍스트 출력
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("사이즈가 맞지 않는 누군가의 방수복이다. 아직 조금 축축하다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }

    public string GetInteractText()
    {
        // 화면에 띄우고 싶은 텍스트를 리턴합니다.
        return "조사: 방수복";
    }
}
using UnityEngine;

public class MapInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip mapSound; // ★ 지도 펼치는 소리 (Paper Rustle, Map Open)
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
        if (mapSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(mapSound);
        }

        // 2. 텍스트 출력
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("홍수 이전 등대 주변의 지도이다. 지금은 아무 쓸모도 없다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
    public string GetInteractText()
    {
        // 화면에 띄우고 싶은 텍스트를 리턴합니다.
        return "조사: 지도 확인";
    }
}
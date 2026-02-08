using UnityEngine;

public class BookCaseInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip bookSound; // ★ 책 넘기는 소리 (Page Flip, Book Thud)
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
        if (bookSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bookSound);
        }

        // 2. 텍스트 출력
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("오래된 항해 일지가 빼곡하게 꽂혀 있다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
    public string GetInteractText()
    {
        // 화면에 띄우고 싶은 텍스트를 리턴합니다.
        return "조사: 책장";
    }
}
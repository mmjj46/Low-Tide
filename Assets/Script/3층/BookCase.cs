using UnityEngine;
public class BookCaseInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip bookSound;
    private AudioSource audioSource;

    private bool hasInteractedOnce = false;

    // ★ 이 단서의 고유 ID (DiaryTabManager에서 확인하는 이름과 똑같아야 합니다!)
    private string clueKey = "Clue_Bookshelf";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Interact()
    {
        // 1. 소리 재생
        if (bookSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bookSound);
        }

        // 2. 텍스트 출력 분기 및 단서 저장
        if (UIManager.Instance != null)
        {
            // 분기 1: 이미 예전에 단서를 찾아서 일기장에 적어둔 상태인가? (PlayerPrefs 확인)
            if (PlayerPrefs.GetInt(clueKey, 0) == 1)
            {
                UIManager.Instance.ShowNotification("책장에서 발견했던 쪽지다. <color=#FFD700>노란색</color>으로 숫자 <color=#FFD700>5</color>가 적혀 있다.");
            }
            // 분기 2: 처음 클릭했을 때 (뜸 들이기)
            else if (!hasInteractedOnce)
            {
                UIManager.Instance.ShowNotification("오래된 항해 일지가 빼곡하게 꽂혀 있다. 그런데...");
                hasInteractedOnce = true;
            }
            // 분기 3: 두 번째 클릭했을 때 (단서 획득 및 저장!)
            else
            {
                string clueMessage = "자세히 보니 두 권의 책 사이에 쪽지가 숨어 있다. <color=#FFD700>노란색</color>으로 숫자 <color=#FFD700>5</color>가 적혀 있다. 중요한 숫자인 것 같아 일기장에 적어 두었다.";
                UIManager.Instance.ShowNotification(clueMessage);
                // ★ 핵심: 이제 일기장(DiaryTabManager)에서 이 단서를 볼 수 있도록 저장합니다!
                PlayerPrefs.SetInt(clueKey, 1);
                PlayerPrefs.Save();
            }
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }

    public string GetInteractText()
    {
        return "조사: 책장";
    }
}
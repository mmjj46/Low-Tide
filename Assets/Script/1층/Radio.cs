using UnityEngine;
public class RadioInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip radioNoiseSound;
    private AudioSource audioSource;

    private bool hasInteractedOnce = false;

    // ★ 이 단서의 고유 ID (DiaryTabManager에서 확인하는 이름과 똑같아야 합니다!)
    private string clueKey = "Clue_Radio";

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
        // 1. 소리 재생 (상호작용할 때마다 지직거리는 소리는 계속 나도록 유지)
        if (radioNoiseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(radioNoiseSound);
        }

        // 2. 텍스트 출력 분기 및 단서 저장
        if (UIManager.Instance != null)
        {
            // 분기 1: 이미 예전에 단서를 찾아서 일기장에 적어둔 상태인가? (PlayerPrefs 확인)
            if (PlayerPrefs.GetInt(clueKey, 0) == 1)
            {
                UIManager.Instance.ShowNotification("라디오에서 발견했던 쪽지다. <color=#00462A>초록색</color>으로 숫자 <color=#00462A>4</color>가 적혀 있다.");
            }
            // 분기 2: 처음 클릭했을 때 (뜸 들이기)
            else if (!hasInteractedOnce)
            {
                UIManager.Instance.ShowNotification("다이얼을 돌려봐도 잡음만 들린다. 그런데...");
                hasInteractedOnce = true;
            }
            // 분기 3: 두 번째 클릭했을 때 (단서 획득 및 저장!)
            else
            {
                string clueMessage = "자세히 보니 배터리를 넣는 부분에 쪽지가 끼어 있다. <color=#00462A>초록색</color>으로 숫자 <color=#00462A>4</color>가 적혀 있다. 중요한 숫자인 것 같아 일기장에 적어 두었다.";
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
        return "조사: 라디오";
    }
}
using UnityEngine;
public class toolboxInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip toolSound;
    private AudioSource audioSource;

    private bool hasInteractedOnce = false;

    // ★ 이 단서의 고유 ID (DiaryTabManager에서 확인하는 이름과 똑같아야 합니다!)
    private string clueKey = "Clue_Toolbox";

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
        if (toolSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(toolSound);
        }

        // 2. 텍스트 출력 분기 및 단서 저장
        if (UIManager.Instance != null)
        {
            // 분기 1: 이미 예전에 단서를 찾아서 일기장에 적어둔 상태인가? (PlayerPrefs 확인)
            if (PlayerPrefs.GetInt(clueKey, 0) == 1)
            {
                UIManager.Instance.ShowNotification("공구함에서 발견했던 쪽지다. <color=#3377FF>파란색</color>으로 숫자 <color=#3377FF>8</color>이 적혀 있다.");
            }
            // 분기 2: 처음 클릭했을 때 (뜸 들이기)
            else if (!hasInteractedOnce)
            {
                UIManager.Instance.ShowNotification("무거운 공구가 가득하다. 절반 정도는 용도를 모르겠다. 그런데...");
                hasInteractedOnce = true;
            }
            // 분기 3: 두 번째 클릭했을 때 (단서 획득 및 저장!)
            else
            {
                string clueMessage = "자세히 보니 공구들 사이에 쪽지가 던져져 있다. <color=#3377FF>파란색</color>으로 숫자 <color=#3377FF>8</color>이 적혀 있다. 중요한 숫자인 것 같아 일기장에 적어 두었다.";
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
        return "조사: 공구상자";
    }
}
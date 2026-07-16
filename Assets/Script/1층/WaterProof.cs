using UnityEngine;

public class WaterProofInteraction : MonoBehaviour, IInteractable
{
    [Header("사운드 설정")]
    public AudioClip interactSound; // ★ 옷 부스럭거리는 소리 or 젖은 소리
    private AudioSource audioSource;

    // 첫 번째 상호작용 체크용 (게임 켜져있는 동안만 임시 기억)
    private bool hasInteractedOnce = false;

    // ★ 이 단서의 고유 ID (DiaryTabManager에서 확인하는 이름과 똑같아야 합니다!)
    private string clueKey = "Clue_Suit";

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
        // 1. 소리 재생
        if (interactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactSound);
        }

        // 2. 텍스트 출력 분기 및 단서 저장
        if (UIManager.Instance != null)
        {
            // 분기 1: 이미 예전에 단서를 찾아서 일기장에 적어둔 상태인가? (PlayerPrefs 확인)
            if (PlayerPrefs.GetInt(clueKey, 0) == 1)
            {
                UIManager.Instance.ShowNotification("방수복에서 발견했던 쪽지다. <color=red>빨간색</color>으로 숫자 <color=red>2</color>가 적혀 있다.");
            }
            // 분기 2: 처음 클릭했을 때 (뜸 들이기)
            else if (!hasInteractedOnce)
            {
                UIManager.Instance.ShowNotification("사이즈가 맞지 않는 누군가의 방수복이다. 아직 조금 축축하다. 그런데...");
                hasInteractedOnce = true; // 다음 클릭을 위해 상태 변경
            }
            // 분기 3: 두 번째 클릭했을 때 (단서 획득 및 저장!)
            else
            {
                string clueMessage = "자세히 보니 주머니에 쪽지가 들어 있다. <color=red>빨간색</color>으로 숫자 <color=red>2</color>가 적혀 있다. 중요한 숫자인 것 같아 일기장에 적어 두었다.";
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
        // 화면에 띄우고 싶은 텍스트를 리턴합니다.
        return "조사: 방수복";
    }
}
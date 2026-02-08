using UnityEngine; // 'Ray'를 인식하기 위해 반드시 필요합니다.
using TMPro;       // TextMeshPro를 사용하기 위해 필요합니다.

// 유니티 스크립트는 반드시 아래와 같은 클래스 구조 안에 있어야 합니다.
public class InteractionController : MonoBehaviour
{
    [Header("설정")]
    public float interactionDistance = 3f;
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;

    [Header("사운드")]
    public AudioClip interactSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // 민정 님이 작성하신 레이캐스트 로직
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (interactionUI != null) interactionUI.SetActive(true);
                if (interactionText != null) interactionText.text = interactable.GetInteractText();

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (audioSource != null && interactSound != null)
                    {
                        audioSource.PlayOneShot(interactSound);
                    }
                    interactable.Interact();
                }
            }
            else
            {
                if (interactionUI != null) interactionUI.SetActive(false);
            }
        }
        else
        {
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }
}
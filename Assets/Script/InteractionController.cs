using UnityEngine;
using TMPro;

public class InteractionController : MonoBehaviour
{
    public float interactionDistance = 3f; // 상호작용 가능한 최대 거리
    public GameObject interactionUI; // 비활성화해둔 상호작용 UI
    public TextMeshProUGUI interactionText; // 상호작용 UI의 텍스트

    [Header("Sound Settings")]
    public AudioClip interactSound; // ★ 1. 여기에 효과음 파일을 드래그해서 넣으세요
    private AudioSource audioSource; // 소리를 재생할 컴포넌트

    void Start()
    {
        // ★ 2. 게임 시작 시 AudioSource 컴포넌트를 가져오거나 없으면 만듭니다.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                interactionUI.SetActive(true);

                // F키 입력 확인
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // ★ 3. 상호작용할 때 소리 재생
                    if (audioSource != null && interactSound != null)
                    {
                        audioSource.PlayOneShot(interactSound); // 효과음 1회 재생
                    }

                    // 레이캐스트에 맞은 오브젝트에게 "Interact" 함수 실행 요청
                    hit.collider.SendMessage("Interact");
                }
            }
            else
            {
                interactionUI.SetActive(false);
            }
        }
        else
        {
            interactionUI.SetActive(false);
        }
    }
}
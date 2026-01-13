using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class InteractionController : MonoBehaviour
{
    public float interactionDistance = 3f; // 상호작용 가능한 최대 거리
    public GameObject interactionUI; // 비활성화해둔 상호작용 UI
    public TextMeshProUGUI interactionText; // 상호작용 UI의 텍스트

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                // UI를 표시하는 부분 (이전과 동일)
                interactionUI.SetActive(true);

                // --- 이 부분이 새로 추가된 부분입니다! ---
                // F키 입력 확인
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // 레이캐스트에 맞은 오브젝트에게 "Interact"라는 이름의 함수를 실행하라고 메시지를 보냅니다.
                    hit.collider.SendMessage("Interact");
                }
                // ------------------------------------
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
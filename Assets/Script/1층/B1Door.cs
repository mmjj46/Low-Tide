using UnityEngine;
using System.Collections;

public class B1Door : MonoBehaviour, IInteractable
{
    private GameManager gameManager;
    private bool hasInteractedOnDay14 = false; // 14일차 상호작용 횟수 체크

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void Interact()
    {
        int currentDay = (gameManager != null) ? gameManager.GetCurrentDay() : PlayerPrefs.GetInt("CurrentDay", 1);

        if (UIManager.Instance == null) return;

        // 1일 ~ 13일
        if (currentDay <= 13)
        {
            UIManager.Instance.ShowNotification("문이 굳게 잠겨 열리지 않는다. 아직은 걱정하지 않아도 될 것 같다.");
        }
        // 14일
        else if (currentDay == 14)
        {
            if (!hasInteractedOnDay14)
            {
                // 14일차 첫 번째 조사
                UIManager.Instance.ShowNotification("문이 굳게 잠겨 열리지 않는다. 그래도 당신은 꼭 이 문을 열어야만 한다.");
                hasInteractedOnDay14 = true;
            }
            else
            {
                // 14일차 두 번째 조사 (추가 피드백)
                UIManager.Instance.ShowNotification("끝까지 믿고 싶지 않았던 진실을, 이제는 두 눈으로 직접 확인해야만 하니까.");

                // 진실을 확인해야 한다는 결심 이후에 이벤트 연출을 시작하고 싶다면 아래 코드를 활성화하세요.
                // StartCoroutine(BasementEventSequence());
            }
        }
    }

    public string GetInteractText()
    {
        return "조사: 지하문";
    }

    // 연출 로직 (필요 시 호출)
    private IEnumerator BasementEventSequence()
    {
        Debug.Log("화면이 까매집니다...");
        yield return new WaitForSeconds(1.0f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("지하문을 열고 안을 들여다본다...");
        }

        yield return new WaitForSeconds(2.0f);
        Debug.Log("화면이 다시 밝아집니다.");
    }
}
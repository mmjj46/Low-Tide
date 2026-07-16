using UnityEngine;

public class MirrorInteraction : MonoBehaviour, IInteractable
{
    private GameManager gameManager;

    void Start()
    {
        // 씬 내의 GameManager를 찾아 할당합니다.
        gameManager = FindObjectOfType<GameManager>();
    }

    public void Interact()
    {
        if (UIManager.Instance != null)
        {
            // gameManager가 있다면 현재 날짜를 가져오고, 없다면 기본값 1을 사용
            int currentDay = (gameManager != null) ? gameManager.GetCurrentDay() : 1;

            // 1일 ~ 10일
            if (currentDay <= 10)
            {
                UIManager.Instance.ShowNotification("당신이다!");
            }
            // 11일 ~ 14일 (혹은 그 이후)
            else
            {
                UIManager.Instance.ShowNotification("그 모든 일이 있었음에도, 여전히 당신이다.");
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
        return "조사: 거울";
    }
}
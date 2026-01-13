using UnityEngine;

public class MorseInteraction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("요즘에는 자판을 치면 컴퓨터가 알아서 모스 부호를 내보내기 때문에 따로 외우고 있지 않아도 된다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
}
using UnityEngine;

public class RadioInteraction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("다이얼을 돌려봐도 잡음만 들린다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
}
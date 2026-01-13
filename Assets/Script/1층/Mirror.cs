using UnityEngine;

public class MirrorInteraction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("당신이다!");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
}
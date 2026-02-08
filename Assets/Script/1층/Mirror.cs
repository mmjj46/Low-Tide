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
    public string GetInteractText()
    {
        // 화면에 띄우고 싶은 텍스트를 리턴합니다.
        return "조사: 거울";
    }
}
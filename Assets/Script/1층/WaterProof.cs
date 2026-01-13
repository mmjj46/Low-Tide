using UnityEngine;

public class WaterProofInteraction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("사이즈가 맞지 않는 누군가의 방수복이다. 아직 조금 축축하다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
}
using UnityEngine;

public class MapInteraction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("홍수 이전 등대 주변의 지도이다. 지금은 아무 쓸모도 없다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
}
using UnityEngine;

public class Window2Interaction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification("끝없는 바다가 눈앞에 펼쳐져 있다.");
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다!");
        }
    }
    public string GetInteractText()
    {
        // 화면에 띄우고 싶은 텍스트를 리턴합니다.
        return "조사: 창문";
    }
}
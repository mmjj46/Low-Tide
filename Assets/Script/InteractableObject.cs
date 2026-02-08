using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("설정")]
    public string objectName;  // 예: "통신기", "발전기"
    public string actionVerb;  // 예: "복구", "조사", "작성"

    public void Interact()
    {
        Debug.Log($"{objectName}과 상호작용했습니다!");
        // 여기에 실제 작동 로직 (예: 발전기 가동 애니메이션 등)을 넣으세요.
    }

    public string GetInteractText()
    {
        // 민정님이 원하시는 형식으로 반환
        return $"조사: {objectName} {actionVerb}";
    }
}
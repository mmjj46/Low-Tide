using UnityEngine;
using UnityEngine.EventSystems; // ★ 이게 있어야 마우스 감지 기능을 씁니다!

// 마우스 올리기(Enter)와 클릭(Click)을 감지하는 기능을 상속받음
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    // 마우스가 버튼 위에 올라갔을 때 자동으로 실행됨
    public void OnPointerEnter(PointerEventData eventData)
    {
        // SoundManager야, 호버 소리 좀 내줘!
        SoundManager.Instance.PlaySFX(SoundManager.Instance.hoverClip);
    }

    // 마우스로 버튼을 클릭했을 때 자동으로 실행됨
    public void OnPointerClick(PointerEventData eventData)
    {
        // SoundManager야, 클릭 소리 좀 내줘!
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
    }
}
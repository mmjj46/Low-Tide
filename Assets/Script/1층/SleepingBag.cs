using UnityEngine;

public class SleepingBag : MonoBehaviour
{
    public void Interact()
    {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        // 미션을 완료하지 않았으면 경고 메시지
        if (!gm.IsTodayMissionComplete())
        {
            UIManager.Instance.ShowNotification("오늘의 미션을 먼저 완료하세요!");
            return;
        }

        // 미션 완료했으면 다음 날로 (메시지 사라진 후 날짜 전환)
        UIManager.Instance.ShowNotification("잠을 자서 다음 날로 넘어갑니다.", () =>
        {
            gm.GoToNextDay();
        });
    }
}
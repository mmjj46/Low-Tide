using System.Collections;
using UnityEngine;
using TMPro;

// 역할: "3초 알림"을 전담합니다. (알림 메시지 충돌 문제 해결사)
public class UIManager : MonoBehaviour
{
    // 1. (연결 3) 3초 알림용 UI 텍스트
    public TextMeshProUGUI notificationText;
    public float displayTime = 3f;

    // '싱글톤' 패턴: 다른 스크립트가 UIManager.Instance로 쉽게 접근하게 함
    public static UIManager Instance { get; private set; }

    private Coroutine currentNotificationCoroutine;

    void Awake()
    {
        // UIManager는 씬에 오직 하나만 존재하도록 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // 다른 모든 스크립트가 "알림"을 띄울 때 이 함수를 호출
    public void ShowNotification(string message, System.Action onComplete = null)
    {
        // 이전에 켜진 3초 타이머(코루틴)가 있다면 즉시 중지
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }

        // 새로운 3초 타이머 시작
        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, onComplete));
    }

    private IEnumerator ShowNotificationCoroutine(string message, System.Action onComplete)
    {
        notificationText.gameObject.SetActive(true);
        notificationText.text = message;

        // (중요) Time.timeScale이 0이어도 작동하는 Realtime 대기
        yield return new WaitForSecondsRealtime(displayTime);

        notificationText.text = "";
        notificationText.gameObject.SetActive(false);

        // 코루틴 종료
        currentNotificationCoroutine = null;

        // (선택적) "잠자기"처럼, 알림이 끝난 후 추가 동작이 필요하면 실행
        onComplete?.Invoke();
    }
}
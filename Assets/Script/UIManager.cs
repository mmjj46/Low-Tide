using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    [Header("설정")]
    [Range(0.5f, 5f)] // ★ 슬라이더로 제한
    public float displayTime = 1f;

    public static UIManager Instance { get; private set; }

    private Coroutine currentNotificationCoroutine;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ★ 필수 컴포넌트 확인
        if (notificationPanel == null)
        {
            Debug.LogError("UIManager: notificationPanel이 연결되지 않았습니다!");
        }

        if (notificationText == null)
        {
            Debug.LogError("UIManager: notificationText가 연결되지 않았습니다!");
        }

        // 시작 시 알림창 비활성화
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    /// <summary>
    /// 알림 메시지를 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="onComplete">알림 종료 후 실행할 콜백</param>
    public void ShowNotification(string message, System.Action onComplete = null)
    {
        // ★ 필수 컴포넌트 체크
        if (notificationText == null)
        {
            Debug.LogError("UIManager: notificationText가 null입니다!");
            onComplete?.Invoke(); // 콜백은 실행
            return;
        }

        // 이전 알림이 진행 중이면 중단
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }

        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, onComplete));
    }

    private IEnumerator ShowNotificationCoroutine(string message, System.Action onComplete)
    {
        // 1. 텍스트 설정
        notificationText.text = message;

        // 2. 패널 활성화
        if (notificationPanel != null)
            notificationPanel.SetActive(true);
        else if (notificationText != null) // ★ 패널이 없으면 텍스트만
            notificationText.gameObject.SetActive(true);

        // 3. 지정된 시간만큼 대기 (Time.timeScale 무시)
        yield return new WaitForSecondsRealtime(displayTime);

        // 4. 패널 비활성화 (자식인 텍스트도 자동으로 꺼짐)
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
        else if (notificationText != null) // ★ 패널이 없으면 텍스트만 끄기
        {
            notificationText.gameObject.SetActive(false);
        }

        // 5. 텍스트 내용 초기화
        notificationText.text = "";

        // 6. 코루틴 참조 해제
        currentNotificationCoroutine = null;

        // 7. 완료 콜백 실행
        onComplete?.Invoke();
    }

    /// <summary>
    /// 현재 표시 중인 알림을 즉시 숨깁니다.
    /// </summary>
    public void HideNotificationImmediate()
    {
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
            currentNotificationCoroutine = null;
        }

        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (notificationText != null)
        {
            notificationText.text = "";
            notificationText.gameObject.SetActive(false);
        }
    }
}
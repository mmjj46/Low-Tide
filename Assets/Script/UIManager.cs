using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    [Header("설정")]
    [Range(0.5f, 5f)]
    public float defaultDisplayTime = 1.5f; // 기본 시간

    public static UIManager Instance { get; private set; }

    private Coroutine currentNotificationCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (notificationPanel == null) Debug.LogError("UIManager: notificationPanel 없음!");
        if (notificationText == null) Debug.LogError("UIManager: notificationText 없음!");

        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    /// <summary>
    /// ★ [핵심] 시간을 직접 지정할 수 있는 함수 (오류 해결 포인트)
    /// duration에 숫자를 넣으면 그 시간만큼만 뜹니다.
    /// </summary>
    public void ShowNotification(string message, float duration = -1f, System.Action onComplete = null)
    {
        // 시간을 안 적었거나(-1), 0보다 작으면 기본 시간(defaultDisplayTime) 사용
        float timeToWait = (duration > 0) ? duration : defaultDisplayTime;

        if (notificationText == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }

        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, timeToWait, onComplete));
    }

    // 기존 코드들과의 호환성을 위한 오버로딩 (시간 안 적었을 때)
    public void ShowNotification(string message, System.Action onComplete)
    {
        ShowNotification(message, -1f, onComplete);
    }

    private IEnumerator ShowNotificationCoroutine(string message, float duration, System.Action onComplete)
    {
        notificationText.text = message;

        if (notificationPanel != null)
            notificationPanel.SetActive(true);
        else if (notificationText != null)
            notificationText.gameObject.SetActive(true);

        // 받아온 시간(duration)만큼 대기
        yield return new WaitForSecondsRealtime(duration);

        if (notificationPanel != null)
            notificationPanel.SetActive(false);
        else if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        notificationText.text = "";
        currentNotificationCoroutine = null;

        onComplete?.Invoke();
    }

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
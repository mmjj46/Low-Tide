using UnityEngine;

public class SleepingBag : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (gm == null)
        {
            Debug.LogError("[SleepingBag] GameManager를 찾을 수 없습니다!");
            return;
        }

        // ★ 일기를 작성했는지 확인
        if (PlayerPrefs.GetInt("DiaryCompleted", 0) != 1)
        {
            UIManager.Instance?.ShowNotification("먼저 일기를 작성해야 잠들 수 있다.");
            return;
        }

        // ★ 일기 작성 완료 후 저장 + 다음 날로
        UIManager.Instance?.ShowNotification("잠을 자서 다음 날로 넘어갑니다.", () =>
        {
            // 저장
            gm.SaveGameData();

            // 다음 날로 진행
            StartCoroutine(GoToNextDayCoroutine(gm));
        });
    }

    System.Collections.IEnumerator GoToNextDayCoroutine(GameManager gm)
    {
        yield return new WaitForSeconds(0.5f);

        // 다음 날로
        int nextDay = gm.GetCurrentDay() + 1;

        // GameManager의 private 변수를 직접 수정할 수 없으므로
        // GameManager에 public 함수를 추가해야 합니다
        // 임시로 PlayerPrefs를 통해 처리
        PlayerPrefs.SetInt("NextDayPending", 1);
        PlayerPrefs.SetInt("DiaryCompleted", 0); // 플래그 초기화
        PlayerPrefs.Save();

        // 씬 재로드
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SleepingBag : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        GameManager gm = FindObjectOfType<GameManager>();

        if (gm == null) return;

        int currentDay = gm.GetCurrentDay();

        // Day 7 이상: 조건 없이 잠
        if (currentDay > 6)
        {
            UIManager.Instance.ShowNotification("잠을 자서 다음 날로 넘어갑니다.");
            StartCoroutine(ProcessSleep(gm));
            return;
        }

        // ★ Day 1~6: 일기 썼는지 확인
        // 일기장(DiaryDialogue)에서 DiaryCompleted를 1로 만들고 왔어야 함
        if (PlayerPrefs.GetInt("DiaryCompleted", 0) != 1)
        {
            UIManager.Instance.ShowNotification("먼저 일기를 작성해야 잠들 수 있다.");
            return;
        }

        // 일기를 썼다면 여기서 잠을 잠
        UIManager.Instance.ShowNotification("잠을 자서 다음 날로 넘어갑니다.");
        StartCoroutine(ProcessSleep(gm));
    }
    public string GetInteractText()
    {
        // 화면에 띄우고 싶은 텍스트를 리턴합니다.
        return "조사: 잠들기";
    }

    IEnumerator ProcessSleep(GameManager gm)
    {
        yield return new WaitForSeconds(2.0f); // 메시지 읽을 시간

        gm.SaveGameData();

        // ★ 다음 날 아침을 위한 설정
        PlayerPrefs.SetInt("NextDayPending", 1); // "다음 날로 넘어가라"는 명령
        PlayerPrefs.SetInt("DiaryCompleted", 0); // "일기 씀" 상태 초기화 (내일을 위해)
        PlayerPrefs.Save();

        // 씬 재로드 (GameManager가 다시 켜지면서 NextDayPending을 보고 Day를 1 올림)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
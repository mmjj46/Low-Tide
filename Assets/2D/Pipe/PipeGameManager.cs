using UnityEngine;
using UnityEngine.SceneManagement; // ★ 1. 씬 이동 필수
using System.Collections;          // ★ 2. 코루틴(시간 지연) 사용

public class PipeGameManager : MonoBehaviour
{
    public static PipeGameManager instance;

    public Pipe1[] pipes;

    public GameObject clearUI;
    public bool isGameOver = false;

    [Header("사운드")]
    public AudioClip clearSound; // ★ 1. 효과음 연결
    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // ★★★ 3. 마우스 커서 보이게 하기 (필수!) ★★★
        // 1인칭 게임에서 넘어왔으면 커서가 잠겨있을 수 있으므로 풀어줍니다.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void CheckWin()
    {
        // 이미 게임이 끝났으면 중복 검사 방지
        if (isGameOver) return;

        foreach (Pipe1 pipe in pipes)
        {
            // 정답(correctStep)과 비교
            if (pipe.IsCorrect(pipe.correctStep) == false)
            {
                return; // 하나라도 틀리면 리턴
            }
        }

        // --- 여기 내려왔다면 모든 파이프가 정답! ---

        Debug.Log("🎉 게임 클리어! 승리!");
        isGameOver = true;

        if (clearUI != null)
            clearUI.SetActive(true);

        // ★ 4. 잠시 대기 후 메인 게임으로 복귀 (코루틴 실행)
        StartCoroutine(ReturnToMainGame());
    }

    IEnumerator ReturnToMainGame()
    {
        // 클리어 UI를 감상할 시간 2초 주기
        if (clearSound != null) audioSource.PlayOneShot(clearSound);
        yield return new WaitForSeconds(1.0f);

        Debug.Log("메인 게임으로 돌아갑니다.");

        // ★ 5. 성공 기록 남기기 (GameManager가 이걸 확인하고 파이프 수리 완료 처리)
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        // ★ 6. GameScene 로드 (이름이 다르다면 수정해주세요!)
        SceneManager.LoadScene("GameScene");
    }
}
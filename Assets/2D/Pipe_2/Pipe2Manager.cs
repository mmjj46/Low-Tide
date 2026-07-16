using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Pipe2Manager : MonoBehaviour
{
    public static Pipe2Manager instance;

    [Header("연결 대상 (자동으로 찾아짐)")]
    public Pipe2Script[] pipes;
    public GameObject clearUI;

    [Header("게임 상태")]
    public bool isGameOver = false;

    [Header("사운드")]
    public AudioClip clearSound;      // 클리어 효과음
    public AudioClip pipeClickSound; // ★ 추가: 파이프 클릭 효과음
    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 1. 마우스 커서 설정
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 2. 씬에 있는 모든 파이프를 자동으로 찾아서 배열에 넣기
        if (pipes == null || pipes.Length == 0)
        {
            pipes = FindObjectsOfType<Pipe2Script>();
        }

        Debug.Log($"파이프 {pipes.Length}개를 감지했습니다.");

        // 3. 시작하자마자 모든 파이프의 초기 상태 검사
        foreach (var pipe in pipes)
        {
            pipe.ForceCheck();
        }
    }

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            DetectAndRotatePipe();
        }
    }

    // ★ 추가: 클릭 소리를 재생하는 함수
    public void PlayClickSound()
    {
        if (isGameOver) return;
        if (pipeClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pipeClickSound);
        }
    }

    void DetectAndRotatePipe()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null)
        {
            Pipe2Script pipe = hit.transform.GetComponentInParent<Pipe2Script>();

            if (pipe != null)
            {
                // 소리 재생 후 회전 실행
                PlayClickSound();
                pipe.RotatePipe();
            }
        }
    }

    public void CheckClear()
    {
        if (pipes == null || pipes.Length == 0)
        {
            Debug.LogWarning("파이프 목록이 비어있어서 클리어 확인을 할 수 없습니다!");
            return;
        }

        foreach (Pipe2Script pipe in pipes)
        {
            if (pipe.isFixed == false) return;
        }

        Debug.Log("🎉 게임 클리어! 축하합니다!");
        isGameOver = true;

        if (clearUI != null)
            clearUI.SetActive(true);

        StartCoroutine(ReturnToMainGame());
    }

    IEnumerator ReturnToMainGame()
    {
        if (clearSound != null) audioSource.PlayOneShot(clearSound);
        yield return new WaitForSeconds(1.0f);

        Debug.Log("메인 게임으로 돌아갑니다.");

        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameScene");
    }

    // ★★★ 추가: Exit 버튼에서 호출할 함수 ★★★
    public void ExitMiniGame()
    {
        Debug.Log("미니게임을 취소하고 메인 게임 씬으로 돌아갑니다.");

        // 성공 기록을 0으로 초기화하고 복귀
        PlayerPrefs.SetInt("MiniGameSuccess", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameScene");
    }
}
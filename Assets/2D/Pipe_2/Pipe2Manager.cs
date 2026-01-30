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

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 1. 마우스 커서 설정
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 2. 씬에 있는 모든 파이프를 자동으로 찾아서 배열에 넣기
        // (혹시 인스펙터가 비어있어도 여기서 채워줍니다)
        if (pipes == null || pipes.Length == 0)
        {
            pipes = FindObjectsOfType<Pipe2Script>();
        }

        Debug.Log($"파이프 {pipes.Length}개를 감지했습니다.");

        // ★ 3. 시작하자마자 모든 파이프의 초기 상태(정답인지 아닌지)를 검사
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

    void DetectAndRotatePipe()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null)
        {
            Pipe2Script pipe = hit.transform.GetComponentInParent<Pipe2Script>();

            if (pipe != null)
            {
                pipe.RotatePipe();
            }
        }
    }

    public void CheckClear()
    {
        // ★★★ 핵심 수정: 파이프 목록이 비어있으면 절대 클리어되지 않음 ★★★
        if (pipes == null || pipes.Length == 0)
        {
            Debug.LogWarning("파이프 목록이 비어있어서 클리어 확인을 할 수 없습니다!");
            return;
        }

        foreach (Pipe2Script pipe in pipes)
        {
            if (pipe.isFixed == false) return; // 하나라도 연결 안 됐으면 여기서 멈춤
        }

        // --- 여기까지 왔다면 모두 연결된 것 ---

        Debug.Log("🎉 게임 클리어! 축하합니다!");
        isGameOver = true;

        if (clearUI != null)
            clearUI.SetActive(true);

        StartCoroutine(ReturnToMainGame());
    }

    IEnumerator ReturnToMainGame()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("메인 게임으로 돌아갑니다.");

        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameScene");
    }
}
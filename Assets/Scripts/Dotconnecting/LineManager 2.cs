using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LineManager : MonoBehaviour
{
    public GameObject linePrefab;
    public LayerMask dotLayer;

    [System.Serializable]
    public class ColorMapping
    {
        public string colorID;
        public Material lineMaterial;
        public float lineWidth = 0.1f;
    }

    [Header("색상 설정")]
    public List<ColorMapping> colorMappings;

    // 새로운 Dot2 리스트를 사용
    private List<Dot2> allDots = new List<Dot2>();
    private Dictionary<string, ColorMapping> colorMapDict = new Dictionary<string, ColorMapping>();

    private Dot2 startDot;
    private GameObject currentLine;
    private LineRenderer currentLineRenderer;

    private bool isGameOver = false;

    [Header("사운드 설정")]
    public AudioClip connectSound; // 연결 성공 시
    public AudioClip failSound;    // 연결 실패 시
    public AudioClip clearSound;   // 레벨 클리어 시
    private AudioSource audioSource;

    void Start()
    {
        // 커서 설정
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // AudioSource 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 컬러 맵 딕셔너리 초기화
        foreach (var mapping in colorMappings)
        {
            if (!colorMapDict.ContainsKey(mapping.colorID))
            {
                colorMapDict.Add(mapping.colorID, mapping);
            }
        }

        // 씬 내의 모든 Dot2 찾기
        Dot2[] foundDots = FindObjectsByType<Dot2>(FindObjectsSortMode.None);
        allDots.AddRange(foundDots);
    }

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleDragStart();
        }
        else if (Input.GetMouseButton(0) && startDot != null)
        {
            HandleDragging();
        }
        else if (Input.GetMouseButtonUp(0) && startDot != null)
        {
            HandleDragEnd();
        }
    }

    private void HandleDragStart()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos, dotLayer);

        if (hitCollider != null)
        {
            Dot2 dot = hitCollider.GetComponent<Dot2>();
            if (dot != null && !dot.isConnected)
            {
                startDot = dot;
                CreateLine();
            }
        }
    }

    private void CreateLine()
    {
        currentLine = Instantiate(linePrefab);
        currentLine.transform.position = Vector3.zero;
        currentLineRenderer = currentLine.GetComponent<LineRenderer>();
        currentLineRenderer.positionCount = 2;

        // 연결 포인트 또는 중심점 계산
        Vector3 startPos = (startDot.connectionPoint != null) ? startDot.connectionPoint.position : startDot.transform.position;
        startPos.z = 0;

        currentLineRenderer.SetPosition(0, startPos);
        currentLineRenderer.SetPosition(1, startPos);

        // 색상 및 두께 적용
        if (colorMapDict.ContainsKey(startDot.colorID))
        {
            ColorMapping mapping = colorMapDict[startDot.colorID];
            currentLineRenderer.material = mapping.lineMaterial;
            currentLineRenderer.startWidth = mapping.lineWidth;
            currentLineRenderer.endWidth = mapping.lineWidth;
        }
    }

    private void HandleDragging()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        currentLineRenderer.SetPosition(1, mousePos);
    }

    private void HandleDragEnd()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos, dotLayer);
        bool connectionSuccessful = false;

        if (hitCollider != null)
        {
            Dot2 endDot = hitCollider.GetComponent<Dot2>();

            // 연결 조건 체크: 다른 점이고, 미연결 상태이며, 색상 ID가 같아야 함
            if (endDot != null && endDot != startDot && !endDot.isConnected && endDot.colorID == startDot.colorID)
            {
                connectionSuccessful = true;

                Vector3 endPos = (endDot.connectionPoint != null) ? endDot.connectionPoint.position : endDot.transform.position;
                endPos.z = 0;

                currentLineRenderer.SetPosition(1, endPos);

                // 데이터 갱신
                startDot.isConnected = true;
                endDot.isConnected = true;

                // 새로운 패키지 기능: 스프라이트 변경
                startDot.ChangeToConnectedSprite();
                endDot.ChangeToConnectedSprite();

                // 사운드 재생
                if (connectSound != null) audioSource.PlayOneShot(connectSound);
            }
        }

        if (!connectionSuccessful)
        {
            Destroy(currentLine);
            if (failSound != null) audioSource.PlayOneShot(failSound);
        }

        startDot = null;
        currentLine = null;
        currentLineRenderer = null;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (isGameOver) return;

        foreach (Dot2 dot in allDots)
        {
            if (!dot.isConnected) return;
        }

        // 승리 조건 달성
        isGameOver = true;
        Debug.Log("🎉 LEVEL COMPLETE!");

        if (clearSound != null) audioSource.PlayOneShot(clearSound);

        // 씬 전환 로직 실행
        StartCoroutine(ReturnToMainGame(0.5f));
    }

    IEnumerator ReturnToMainGame(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log("메인 게임으로 돌아갑니다.");

        // 미니게임 성공 결과 저장
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        // 씬 로드 (씬 이름이 "GameScene"인지 확인하세요)
        SceneManager.LoadScene("GameScene");
    }
}
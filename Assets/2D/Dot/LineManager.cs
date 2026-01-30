using UnityEngine;
using UnityEngine.SceneManagement; // ★ 1. 씬 이동을 위해 필수!
using System.Collections;
using System.Collections.Generic;

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

    public List<ColorMapping> colorMappings;
    private List<Dot> allDots = new List<Dot>();
    private Dictionary<string, ColorMapping> colorMapDict = new Dictionary<string, ColorMapping>();

    private Dot startDot;
    private GameObject currentLine;
    private LineRenderer currentLineRenderer;

    void Start()
    {
        // ★★★ 2. 마우스 커서 잠금 해제 (필수!) ★★★
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        foreach (var mapping in colorMappings)
        {
            if (!colorMapDict.ContainsKey(mapping.colorID))
            {
                colorMapDict.Add(mapping.colorID, mapping);
            }
        }

        Dot[] foundDots = FindObjectsByType<Dot>(FindObjectsSortMode.None);
        allDots.AddRange(foundDots);

        Debug.Log($"[게임 시작] 발견된 점 개수: {allDots.Count}개");
    }

    void Update()
    {
        // 마우스 클릭 시 로그 출력
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
            Dot dot = hitCollider.GetComponent<Dot>();
            if (dot != null)
            {
                if (!dot.isConnected)
                {
                    startDot = dot;
                    CreateLine(startDot.transform.position, dot.colorID);
                }
            }
        }
    }

    private void CreateLine(Vector3 startPos, string colorID)
    {
        currentLine = Instantiate(linePrefab);
        currentLine.transform.position = Vector3.zero;
        currentLineRenderer = currentLine.GetComponent<LineRenderer>();
        currentLineRenderer.positionCount = 2;
        startPos.z = 0;
        currentLineRenderer.SetPosition(0, startPos);
        currentLineRenderer.SetPosition(1, startPos);

        if (colorMapDict.ContainsKey(colorID))
        {
            ColorMapping mapping = colorMapDict[colorID];
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
            Dot endDot = hitCollider.GetComponent<Dot>();
            if (endDot != null && endDot != startDot && !endDot.isConnected && endDot.colorID == startDot.colorID)
            {
                connectionSuccessful = true;
                Vector3 endPos = endDot.transform.position;
                endPos.z = 0;
                currentLineRenderer.SetPosition(1, endPos);
                startDot.isConnected = true;
                endDot.isConnected = true;
                Debug.Log($"[연결 성공] {startDot.name} <-> {endDot.name}");
            }
        }

        if (!connectionSuccessful)
        {
            Destroy(currentLine);
            Debug.Log("[연결 실패] 선이 삭제되었습니다.");
        }

        startDot = null;
        currentLine = null;
        currentLineRenderer = null;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        foreach (Dot dot in allDots)
        {
            if (!dot.isConnected) return;
        }

        Debug.Log("LEVEL COMPLETE! 2초 뒤 메인 게임으로 돌아갑니다.");

        // 더 이상 조작 못하게 스크립트 비활성화
        this.enabled = false;

        // ★ 3. 메인 게임 복귀 코루틴 실행
        StartCoroutine(ReturnToMainGame());
    }

    // ★ 4. 메인 게임 복귀 및 저장 로직
    IEnumerator ReturnToMainGame()
    {
        // 2초 대기 (완료된 화면 감상)
        yield return new WaitForSeconds(0.5f);

        Debug.Log("메인 게임으로 이동 중...");

        // 성공 기록 저장 (GameManager가 확인용)
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();

        // GameScene 로드 (씬 이름이 다르다면 수정하세요!)
        SceneManager.LoadScene("GameScene");
    }
}
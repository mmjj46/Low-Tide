using UnityEngine;
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
            Debug.Log($"[클릭 성공] 감지된 오브젝트: {hitCollider.name}");

            Dot dot = hitCollider.GetComponent<Dot>();
            if (dot != null)
            {
                if (!dot.isConnected)
                {
                    Debug.Log($" -> 시작점 설정 완료: {dot.name} ({dot.colorID})");
                    startDot = dot;
                    CreateLine(startDot.transform.position, dot.colorID);
                }
                else
                {
                    Debug.Log($" -> 실패: {dot.name}은 이미 연결된 상태입니다.");
                }
            }
            else
            {
                Debug.Log($" -> 실패: {hitCollider.name}에는 Dot 스크립트가 없습니다.");
            }
        }
        else
        {
            // 여기가 계속 뜬다면 Collider나 Layer 문제
            Debug.Log($"[클릭 실패] 허공을 클릭했습니다. (마우스 위치: {mousePos})");
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

        // 변경점: OverlapPoint 사용
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

        Debug.Log("LEVEL COMPLETE! 2초 뒤 게임을 종료합니다.");
        this.enabled = false;
        StartCoroutine(QuitGameAfterDelay(2.0f));
    }

    IEnumerator QuitGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
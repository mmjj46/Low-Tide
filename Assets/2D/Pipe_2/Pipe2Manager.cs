using UnityEngine;

public class Pipe2Manager : MonoBehaviour
{
    public static Pipe2Manager instance;

    [Header("연결 대상")]
    public Pipe2Script[] pipes;
    public GameObject clearUI;

    [Header("게임 상태")]
    public bool isGameOver = false;

    void Awake()
    {
        instance = this;
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
                Debug.Log($"파이프 찾음: {pipe.name} -> 회전 시도"); // 확인용 로그
                pipe.RotatePipe();
            }
            else
            {
                Debug.Log($"클릭은 했으나 파이프 스크립트가 없음: {hit.name}");
            }
        }
    }

    public void CheckClear()
    {
        foreach (Pipe2Script pipe in pipes)
        {
            if (pipe.isFixed == false) return;
        }

        Debug.Log("🎉 게임 클리어! 축하합니다!");
        isGameOver = true;

        if (clearUI != null)
            clearUI.SetActive(true);
    }
}
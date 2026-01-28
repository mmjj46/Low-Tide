using UnityEngine;

public class TouchManager : MonoBehaviour
{
    void Update()
    {
        // 1. 마우스 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            // 2. 마우스 위치를 월드 좌표로 변환
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 3. 그 위치에 있는 "Collider"를 검거
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null)
            {
                // 4. 맞은 녀석이 'Pipe'라는 부품(스크립트)을 가지고 있는지 확인
                Pipe1 clickedPipe = hit.GetComponent<Pipe1>();

                if (clickedPipe != null)
                {
                    // 5. 파이프 돌리기
                    clickedPipe.RotateByManager();
                }
            }
        }
    }
}
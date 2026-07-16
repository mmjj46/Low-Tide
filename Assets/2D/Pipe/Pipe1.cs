using UnityEngine;

public class Pipe1 : MonoBehaviour
{
    public int currentStep = 0;
    public int correctStep;

    public void RotateByManager()
    {
        // 1. 정답이면 회전하지 않음
        if (IsCorrect(correctStep))
        {
            Debug.Log($"{name}은(는) 이미 고정되었습니다!");
            return;
        }

        // 2. 파이프 회전
        transform.Rotate(0, 0, -90);
        currentStep = (currentStep + 1) % 4;

        // 3. 매니저에게 명령 전달
        if (PipeGameManager.instance != null)
        {
            // ★ 추가: 매니저에 있는 소리 재생 함수를 먼저 실행!
            PipeGameManager.instance.PlayClickSound();

            // 승리 체크 실행
            PipeGameManager.instance.CheckWin();
        }
    }

    public virtual bool IsCorrect(int targetStep)
    {
        return currentStep == targetStep;
    }
}
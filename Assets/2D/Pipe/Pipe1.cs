using UnityEngine;

public class Pipe1 : MonoBehaviour
{
    public int currentStep = 0;
    public int correctStep;

    public void RotateByManager()
    {
        if (IsCorrect(correctStep))
        {
            Debug.Log($"{name}은(는) 이미 고정되었습니다!");
            return;
        }

        transform.Rotate(0, 0, -90);
        currentStep = (currentStep + 1) % 4;

        if (PipeGameManager.instance != null)
            PipeGameManager.instance.CheckWin();
    }

    public virtual bool IsCorrect(int targetStep)
    {
        return currentStep == targetStep;
    }
}
using UnityEngine;

public class PipeGameManager : MonoBehaviour
{
    public static PipeGameManager instance;

    public Pipe1[] pipes;

    public GameObject clearUI;
    public bool isGameOver = false;

    void Awake()
    {
        instance = this;
    }

    public void CheckWin()
    {
        foreach (Pipe1 pipe in pipes)
        {
            // 정답(correctStep)과 비교
            if (pipe.IsCorrect(pipe.correctStep) == false)
            {
                return;
            }
        }

        Debug.Log("🎉 게임 클리어! 승리!");
        isGameOver = true;

        if (clearUI != null)
            clearUI.SetActive(true);
    }
}
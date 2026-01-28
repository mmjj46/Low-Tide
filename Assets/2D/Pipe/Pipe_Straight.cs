using UnityEngine;

// Pipe 기능을 물려받음 (: Pipe)
public class Pipe_Straight : Pipe1
{
    
    public override bool IsCorrect(int targetStep)
    {
        
        return (currentStep % 2) == (targetStep % 2);
    }
}
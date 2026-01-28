using UnityEngine;
using System.Linq;

public class Pipe2Script : MonoBehaviour
{
    [Header("설정값")]
    public int currentStep = 0;
    public int[] correctSteps; // 정답 리스트

    [Header("상태 확인용")]
    public bool isFixed = false;

    private float lastRotateTime = 0f;
    private float rotateCoolTime = 0.2f;

    void Start()
    {
        // 시작할 때 정답 리스트가 비어있으면 경고
        if (correctSteps == null || correctSteps.Length == 0)
        {
            correctSteps = new int[] { 0 }; // 강제로 0을 넣음
        }
    }

    public void RotatePipe()
    {
        if (isFixed) return;
        if (Time.time - lastRotateTime < rotateCoolTime) return;

        lastRotateTime = Time.time;

        // 회전 및 값 증가
        transform.Rotate(0, 0, -90);
        currentStep++;

        // 4가 되면 0으로 초기화
        if (currentStep >= 4) currentStep = 0;

        // [진단 로그] 클릭할 때마다 현재 상태와 정답을 콘솔에 띄웁니다.
        string answerString = string.Join(", ", correctSteps); // 정답 목록을 문자로 변환
        Debug.Log($" {name} 클릭됨 | 현재: {currentStep} | 정답: [{answerString}]");

        CheckIsCorrect();
    }

    void CheckIsCorrect()
    {
        if (correctSteps.Contains(currentStep))
        {
            isFixed = true;
            Debug.Log($" {name}: 정답! (현재 {currentStep} == 정답 {correctSteps[0]})");

            if (Pipe2Manager.instance != null)
                Pipe2Manager.instance.CheckClear();
        }
    }
}
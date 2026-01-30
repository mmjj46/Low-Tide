using UnityEngine;
using System.Linq;

public class Pipe2Script : MonoBehaviour
{
    [Header("설정값")]
    public int currentStep = 0;
    public int[] correctSteps; // 정답 각도들

    [Header("상태 확인용")]
    public bool isFixed = false;

    private float lastRotateTime = 0f;
    private float rotateCoolTime = 0.2f;

    void Start()
    {
        // 안전장치: 정답 입력 안 했으면 0을 정답으로
        if (correctSteps == null || correctSteps.Length == 0)
        {
            correctSteps = new int[] { 0 };
        }
    }

    // ★ 매니저가 게임 시작할 때 호출하는 함수
    public void ForceCheck()
    {
        CheckIsCorrect(false); // 초기화 때는 매니저에게 '게임 끝났니?'라고 묻지 않음 (무한루프 방지)
    }

    public void RotatePipe()
    {
        if (isFixed) return; // 이미 맞춘 건 못 돌리게 하려면 유지 (계속 돌리게 하려면 이 줄 삭제)
        if (Time.time - lastRotateTime < rotateCoolTime) return;

        lastRotateTime = Time.time;

        // 회전 및 값 증가
        transform.Rotate(0, 0, -90);
        currentStep++;

        if (currentStep >= 4) currentStep = 0;

        // 돌렸으니까 맞았는지 확인 (매니저에게 알림)
        CheckIsCorrect(true);
    }

    // checkGameClear: 매니저에게 전체 검사를 요청할지 여부
    void CheckIsCorrect(bool notifyManager)
    {
        if (correctSteps.Contains(currentStep))
        {
            isFixed = true; // 정답!
            // Debug.Log($" {name}: 연결됨!");
        }
        else
        {
            isFixed = false; // ★ 오답! (이게 있어야 돌리다가 틀리면 다시 false가 됨)
        }

        // 매니저에게 전체 확인 요청
        if (notifyManager && Pipe2Manager.instance != null)
        {
            Pipe2Manager.instance.CheckClear();
        }
    }
}
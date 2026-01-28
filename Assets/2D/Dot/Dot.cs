using UnityEngine;

/// <summary>
/// 색상 점에 부착하는 스크립트
/// 자신의 색상 ID와 연결 상태를 저장.
/// </summary>
public class Dot : MonoBehaviour
{
    // 인스펙터에서 설정할 색상
    [Tooltip("색상")]
    public string colorID;

    // 이 점이 현재 성공적으로 연결되었는지 여부
    [HideInInspector] // 인스펙터에는 보이지 않도록 설정
    public bool isConnected = false;
}


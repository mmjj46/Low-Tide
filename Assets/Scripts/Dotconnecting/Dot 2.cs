using UnityEngine;

/// <summary>
/// 색상 점(Dot)의 데이터를 관리하고 연결 시 비주얼을 변경하는 스크립트
/// </summary>
public class Dot2 : MonoBehaviour
{
    [Header("기본 설정")]
    [Tooltip("이 점의 고유 색상 ID (예: 'red', 'blue')")]
    public string colorID;

    [Tooltip("선이 연결될 정확한 위치 (비워두면 이 오브젝트의 중심을 사용)")]
    public Transform connectionPoint;

    [Header("비주얼 설정")]
    [Tooltip("연결 성공 시 교체될 스프라이트 이미지")]
    public Sprite connectedSprite;

    [HideInInspector]
    public bool isConnected = false;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // 이미지 변경을 위해 SpriteRenderer 컴포넌트를 가져옵니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// LineManager에서 호출: 연결 성공 시 점의 이미지를 변경합니다.
    /// </summary>
    public void ChangeToConnectedSprite()
    {
        if (connectedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = connectedSprite;
        }
    }
}
using UnityEngine;

// 이 스크립트를 넣으면 자동으로 SpriteRenderer가 있는지 확인합니다.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSwitch : MonoBehaviour
{
    [Header("변경할 스프라이트 설정")]
    public Sprite targetSprite; // 성공 시 바뀔 이미지 (켜진 스위치)

    private SpriteRenderer myRenderer;

    // 이미지가 바뀌었는지 확인하는 변수
    public bool isSpriteChanged { get; private set; } = false;

    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        // 1. 이미 켜진 상태면 무시
        if (isSpriteChanged) return;

        // 2. GameManager 확인
        if (RandomManager.Instance == null)
        {
            Debug.LogError("오류: 씬에 GameManager가 없습니다!");
            return;
        }

        // 3. 확률 계산
        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= RandomManager.Instance.changeChance)
        {
            ChangeSprite();
        }
        else
        {
            Debug.Log("실패! (확률: " + RandomManager.Instance.changeChance + "%)");
        }
    }

    void ChangeSprite()
    {
        if (targetSprite != null)
        {
            myRenderer.sprite = targetSprite;
        }
        else
        {
            Debug.LogWarning(name + "에 Target Sprite가 비어있습니다!");
        }

        isSpriteChanged = true;

        // 게임 클리어 체크
        RandomManager.Instance.CheckGameClear();
    }
}
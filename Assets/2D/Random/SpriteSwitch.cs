using UnityEngine;

// 이 스크립트를 넣으면 자동으로 SpriteRenderer와 AudioSource가 스위치에 추가됩니다!
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class SpriteSwitch : MonoBehaviour
{
    [Header("변경할 스프라이트 설정")]
    public Sprite targetSprite; // 성공 시 바뀔 이미지 (켜진 스위치)

    [Header("사운드 설정")]
    public AudioClip successSound; // ★ 성공 시 날 소리
    public AudioClip failSound;    // ★ 실패 시 날 소리

    private SpriteRenderer myRenderer;
    private AudioSource myAudioSource; // ★ 스피커 역할

    // 이미지가 바뀌었는지 확인하는 변수
    public bool isSpriteChanged { get; private set; } = false;

    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myAudioSource = GetComponent<AudioSource>(); // ★ 스피커 부품 가져오기
    }

    void OnMouseDown()
    {
        // 1. 이미 켜진 상태면 무시 (이미 켜진 상태에서도 소리가 나게 하고 싶다면 이 줄을 아래로 내리면 됩니다)
        if (isSpriteChanged) return;

        // 2. GameManager(RandomManager) 확인
        if (RandomManager.Instance == null)
        {
            Debug.LogError("오류: 씬에 RandomManager가 없습니다!");
            return;
        }

        // 3. 확률 계산
        float randomValue = Random.Range(0f, 100f);

        // ★ 성공했을 때
        if (randomValue <= RandomManager.Instance.changeChance)
        {
            if (successSound != null && myAudioSource != null)
            {
                myAudioSource.PlayOneShot(successSound); // 성공 사운드 재생
            }
            ChangeSprite();
        }
        // ★ 실패했을 때
        else
        {
            if (failSound != null && myAudioSource != null)
            {
                myAudioSource.PlayOneShot(failSound); // 실패 사운드 재생
            }
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
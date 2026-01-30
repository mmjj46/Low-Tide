using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("기본 설정")]
    public float stepInterval = 0.5f;
    [Range(0f, 1f)] public float volume = 0.5f;

    [Header("소리 목록")]
    public AudioClip[] floorSounds;  // 일반 바닥 소리
    public AudioClip[] stairSounds;  // 계단 소리

    private CharacterController characterController;
    private AudioSource audioSource;
    private float stepTimer;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (characterController != null && characterController.isGrounded && characterController.velocity.sqrMagnitude > 0.2f)
        {
            PlayFootstep();
        }
        else
        {
            stepTimer = stepInterval;
        }
    }

    void PlayFootstep()
    {
        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            // 1. 발 밑에 뭐가 있는지 검사 (Raycast)
            RaycastHit hit;
            // 내 위치에서 아래쪽으로 2m 정도 레이저를 쏴봄
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2.0f))
            {
                // 2. 밟은 물체의 태그 확인
                if (hit.collider.CompareTag("Stairs"))
                {
                    // 계단이면 계단 소리 재생
                    PlayRandomSound(stairSounds);
                }
                else
                {
                    // 그 외(Untagged 포함)는 일반 바닥 소리 재생
                    PlayRandomSound(floorSounds);
                }
            }

            stepTimer = 0f;
        }
    }

    // 소리 배열에서 하나 골라 재생하는 함수
    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            int randIndex = Random.Range(0, clips.Length);
            audioSource.pitch = Random.Range(0.9f, 1.1f); // 피치 약간 랜덤
            audioSource.PlayOneShot(clips[randIndex], volume);
        }
    }
}
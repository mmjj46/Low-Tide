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

    // ★ 핵심 무기: 진짜 이동 거리를 재기 위한 변수
    private Vector3 lastPosition;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 시작할 때 내 위치 기억하기
        lastPosition = transform.position;
    }

    void Update()
    {
        // ★ 1. 플레이어가 진짜로 이동 키(WASD, 방향키)를 누르고 있는지 확인!
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isPressingKey = Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f;

        // ★ 2. 미세한 떨림을 무시하고, 진짜로 위치가 변하고 있는지 잰다!
        float movedDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                               new Vector3(lastPosition.x, 0, lastPosition.z));
        bool isMoving = movedDistance > 0.001f;

        // ★ 핵심: 키보드도 누르고 있고, 실제로 위치도 변할 때만 "걷는 중"으로 인정!!
        bool isActuallyWalking = isPressingKey && isMoving;

        // 1. 땅에 닿아있고, 진짜로 걷고 있을 때만 타이머 굴리기
        if (characterController != null && characterController.isGrounded && isActuallyWalking)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        // 2. 키보드에서 손을 뗐거나 벽에 막혀서 못 가면 타이머를 꽉 채워둠 (소리 즉시 멈춤)
        else if (!isActuallyWalking)
        {
            stepTimer = stepInterval;

            // ★ 강제 종료 추가: 멈췄는데 소리가 계속 나고 있다면 자비 없이 꺼버립니다!
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        // 다음 프레임 비교를 위해 현재 위치를 다시 저장
        lastPosition = transform.position;
    }

    void PlayFootstep()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 0.5f, Vector3.down, 2.0f);
        bool soundPlayed = false;

        foreach (RaycastHit hit in hits)
        {
            // 맞은 물체가 '플레이어 자신'이 아니고, 투명한 구역(Trigger)이 아닐 때만!
            if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
            {
                if (hit.collider.CompareTag("Stairs"))
                {
                    PlayRandomSound(stairSounds);
                }
                else
                {
                    PlayRandomSound(floorSounds);
                }
                soundPlayed = true;
                break; // 바닥을 찾아서 소리를 냈으니 반복문 즉시 종료!
            }
        }

        // 만약 예외 상황이라 아무것도 못 맞췄다면 일단 일반 바닥 소리라도 재생
        if (!soundPlayed)
        {
            PlayRandomSound(floorSounds);
        }
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            int randIndex = Random.Range(0, clips.Length);
            audioSource.pitch = Random.Range(0.9f, 1.1f); // 피치 랜덤 (소리가 자연스러워짐)

            // ★ 핵심 수정: 끝날 때까지 멈추지 않는 PlayOneShot 대신, 통제 가능한 Play로 변경!
            audioSource.clip = clips[randIndex];
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}
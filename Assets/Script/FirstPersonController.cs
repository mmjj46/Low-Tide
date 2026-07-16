using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float speed = 3f;
    public float mouseSensitivity = 1f;
    public float gravity = -9.81f;
    public float jumpHeight = 1f;

    private CharacterController controller;
    private Transform playerCamera;
    private float xRotation = 0f;

    private Vector3 velocity; // 중력 및 점프 수직 속도

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. 마우스 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // 2. 바닥 체크 및 중력 초기화
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 땅에 붙어있을 때 안정적으로 하향 벡터 유지
        }

        // 3. 이동 입력 방향 계산 (이 단계에서는 Move하지 않음)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;

        // 4. 점프 실행 (Input.GetKeyDown과 KeyCode.Space로 직관적으로 변경)
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("[FirstPersonController] 점프 성공! 수직 속도 증가");
        }

        // 5. 중력 지속 누적
        velocity.y += gravity * Time.deltaTime;

        // 6. ★★★ 핵심 수정: 수평 이동과 수직 속도(중력/점프)를 하나로 합쳐서 호출 ★★★
        // 이렇게 Move를 단 한 번만 호출해야 isGrounded가 정상 작동합니다.
        Vector3 finalMove = (move * speed) + velocity;
        controller.Move(finalMove * Time.deltaTime);
    }
}
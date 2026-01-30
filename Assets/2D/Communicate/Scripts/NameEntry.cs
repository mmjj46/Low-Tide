using UnityEngine;
using TMPro; // TextMeshPro용 (일반 InputField라면 UnityEngine.UI 사용)

public class NameEntryUIController : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TMP_InputField nameInputField; // 이름 입력창
    public Animator uiAnimator;           // UI 패널의 애니메이터

    // Animator에서 사용할 파라미터 이름
    private const string OUTRO_TRIGGER = "HideUI";

    void Start()
    {
        // 시작하자마자 등장 애니메이션은 Animator의 Entry에서 자동으로 시작되게 설정합니다.
        // 필요하다면 여기서 초기화를 수행합니다.
    }

    // [버튼 연결용] 확인 버튼을 누르면 호출
    public void OnSubmitButtonPressed()
    {
        string userName = nameInputField.text;

        // 이름이 비어있으면 진행하지 않음
        if (string.IsNullOrEmpty(userName))
        {
            Debug.Log("이름을 입력해주세요.");
            return;
        }

        Debug.Log($"입력된 이름: {userName}");
        // TODO: 여기서 이름을 저장하세요 (예: GameManager.Instance.playerName = userName;)

        // 퇴장 애니메이션 재생 (Animator의 Trigger 발동)
        if (uiAnimator != null)
        {
            uiAnimator.SetTrigger(OUTRO_TRIGGER);
        }
    }

    // [애니메이션 이벤트용] 퇴장 애니메이션이 완전히 끝났을 때 호출
    // Animation 창에서 Outro 클립의 마지막 프레임에 이 이벤트를 추가하세요.
    public void OnOutroFinished()
    {
        Debug.Log("UI 퇴장 완료. 다음 씬으로 넘어가거나 게임을 시작합니다.");

        // 예: 패널 비활성화
        gameObject.SetActive(false);

        // 또는 씬 전환: SceneManager.LoadScene("NextScene");
    }
}
using UnityEngine;
using TMPro;
using System.Collections; // ★ 코루틴(IEnumerator) 사용을 위해 추가
using UnityEngine.SceneManagement;

public class ResponderUIManager : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public TMP_InputField nameInputField;

    [Header("애니메이터 연결")]
    public Animator inputWindowAnimator;
    public Animator buttonAnimator;

    [Header("설정")]
    public string idleStateName = "Input_Idle";
    public string gameSceneName = "gamescene";

    // 👇 추가: UI가 사라지는 애니메이션이 재생되는 시간 (인스펙터에서 조절 가능)
    [Header("씬 전환 대기 시간")]
    public float outroDuration = 0.5f;

    private const int MAX_NAME_LENGTH = 5;
    private const string OUTRO_TRIGGER = "HideUI";
    private const string RESPONDER_NAME_KEY = "ResponderName";
    private bool isInputReady = false;
    private bool isSubmitting = false; // 중복 클릭 방지용 플래그

    void Start()
    {
        nameInputField.characterLimit = MAX_NAME_LENGTH;
        nameInputField.onSubmit.AddListener(OnInputFieldSubmit);

        nameInputField.text = "";
        nameInputField.interactable = false;
    }

    void Update()
    {
        if (isInputReady) return;

        if (inputWindowAnimator != null)
        {
            AnimatorStateInfo stateInfo = inputWindowAnimator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(idleStateName))
            {
                ActivateInput();
            }
        }
    }

    void ActivateInput()
    {
        isInputReady = true;
        nameInputField.interactable = true;
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    private void OnInputFieldSubmit(string text)
    {
        OnSubmitButtonPressed();
    }

    public void OnSubmitButtonPressed()
    {
        // 입력이 준비되지 않았거나 이미 제출 중이면 중복 실행 방지
        if (!isInputReady || isSubmitting) return;

        string inputName = nameInputField.text;

        if (string.IsNullOrEmpty(inputName))
        {
            Debug.Log("이름을 입력해주세요!");
            nameInputField.Select();
            nameInputField.ActivateInputField();
            return;
        }

        isSubmitting = true; // 제출 시작 플래그 On
        Debug.Log($"상대방 이름 확정: {inputName}");

        PlayerPrefs.SetString(RESPONDER_NAME_KEY, inputName);
        PlayerPrefs.Save();

        // 애니메이션 트리거 작동
        if (inputWindowAnimator != null) inputWindowAnimator.SetTrigger(OUTRO_TRIGGER);
        if (buttonAnimator != null) buttonAnimator.SetTrigger(OUTRO_TRIGGER);

        // 👇 변경: 애니메이션 이벤트를 기다리지 않고, 코루틴으로 지정된 시간 뒤에 씬 전환을 수행합니다.
        StartCoroutine(WaitAndLoadScene());
    }

    // 지정된 시간(outroDuration)만큼 기다린 후 씬을 넘기는 루틴
    private IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(outroDuration);

        Debug.Log($"[ResponderUIManager] 아웃트로 시간 만료. {gameSceneName} 씬으로 복귀합니다.");
        SceneManager.LoadScene(gameSceneName);
    }
}
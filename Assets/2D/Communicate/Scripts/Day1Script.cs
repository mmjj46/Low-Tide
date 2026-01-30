using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Day1Script : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextAsset dialogueFile;

    private string[] lines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    public GameObject b1;
    public GameObject b2;
    public GameObject b3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        b1.SetActive(false);
        b2.SetActive(false);
        b3.SetActive(false);

        if (dialogueFile != null)
        {
            lines = dialogueFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            isDialogueActive = true;

            ShowNextLine();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
        }
    }
    void ShowNextLine()
    {
        // 더 보여줄 대사가 남아있는지 확인
        if (lines != null && currentLineIndex < lines.Length)
        {
            // 현재 순서의 줄을 텍스트 UI에 표시
            dialogueText.text = lines[currentLineIndex];

            // 다음 줄을 가리키도록 인덱스 증가
            currentLineIndex++;
        }
        else
        {
            EndDialogue();
        }
    }
    void EndDialogue()
    {
        Debug.Log("대사가 끝났습니다. 씬을 닫습니다.");
        isDialogueActive = false;

        // 유니티 에디터에서 실행 중일 때는 플레이 모드를 멈춤
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems; // 클릭 이벤트 감지를 위해 필요

public class ClueInput : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text[] numberTexts;
    private int[] currentDigits = { 0, 0, 0, 0 };
    private int[] correctPassword = { 2, 8, 5, 4 };
    private int selectedIndex = -1;

    [Header("사운드 설정")]
    public AudioClip clickSound;
    public AudioClip successSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        UpdateUISelection();
    }

    // 마우스 클릭 시 해당 칸 선택 (각 텍스트 오브젝트에 이 스크립트가 붙어있거나, EventTrigger 활용)
    public void OnPointerClick(PointerEventData eventData)
    {
        for (int i = 0; i < numberTexts.Length; i++)
        {
            if (numberTexts[i].gameObject == eventData.pointerCurrentRaycast.gameObject)
            {
                selectedIndex = i;
                UpdateUISelection();
                break;
            }
        }
    }

    void Update()
    {
        // 1. 화살표 키로 칸 이동
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { selectedIndex = Mathf.Max(0, selectedIndex - 1); UpdateUISelection(); }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { selectedIndex = Mathf.Min(3, selectedIndex + 1); UpdateUISelection(); }

        // 2. 키보드 숫자 입력 (0-9)
        if (selectedIndex != -1)
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    currentDigits[selectedIndex] = i;
                    numberTexts[selectedIndex].text = i.ToString();
                    PlayClickSound();
                    if (CheckPassword()) StartCoroutine(OnSuccessRoutine());
                }
            }
        }
    }

    void UpdateUISelection()
    {
        for (int i = 0; i < numberTexts.Length; i++)
        {
            numberTexts[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null) audioSource.PlayOneShot(clickSound);
    }

    private bool CheckPassword()
    {
        for (int i = 0; i < 4; i++)
        {
            if (currentDigits[i] != correctPassword[i]) return false;
        }
        return true;
    }

    private IEnumerator OnSuccessRoutine()
    {
        if (successSound != null && audioSource != null) audioSource.PlayOneShot(successSound);
        yield return new WaitForSeconds(successSound != null ? successSound.length : 0.4f);
        PlayerPrefs.SetInt("MiniGameSuccess", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }
}
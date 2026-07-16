using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiaryTabManager : MonoBehaviour
{
    [Header("패널 연결")]
    public GameObject diaryPanel;
    public GameObject hintPanel;

    [Header("탭 버튼의 Image 컴포넌트")]
    public Image diaryTabImage;
    public Image hintTabImage;

    [Header("바꿔낄 이미지(Sprite) 소스")]
    public Sprite diaryOnSprite;
    public Sprite diaryOffSprite;
    public Sprite hintOnSprite;
    public Sprite hintOffSprite;

    [Header("단서 텍스트 박스 (각각 연결)")]
    public TMP_Text hintText_Radio;
    public TMP_Text hintText_Suit;
    public TMP_Text hintText_Toolbox;
    public TMP_Text hintText_Bookshelf;

    private int currentDay;
    private bool isMissionComplete;

    void Start()
    {
        // 1. 현재 게임 상태 불러오기
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        isMissionComplete = PlayerPrefs.GetInt("IsTodayMissionComplete", 0) == 1;

        // ★ 핵심 로직 수정: 
        // 6일 차 이하이면서 "미션까지 완료했을 때"만 일기장을 기본으로 보여줍니다.
        if (currentDay <= 6 && isMissionComplete)
        {
            ClickDiaryTab();
        }
        else
        {
            // 미션을 안 깼거나, 7일 차 이상이면 무조건 단서 패널이 먼저 보입니다!
            ForceClickHintTab();
        }
    }

    // '일기장' 영역을 눌렀을 때
    public void ClickDiaryTab()
    {
        // ★ 주석 해제 및 수정: 미션을 안 깼으면 일기장 탭 진입을 막아냅니다!
        if (!isMissionComplete)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification("아직 일기를 쓸 때가 아니다. 수집한 단서만 확인하자.");
            }
            else
            {
                Debug.Log("아직 일기를 쓸 때가 아니다.");
            }

            // 일기장으로 못 가게 막고, 단서 탭 상태를 유지합니다.
            ForceClickHintTab();
            return;
        }

        // 1. 패널 전환 (정상 진입)
        diaryPanel.SetActive(true);
        hintPanel.SetActive(false);

        // 2. 탭 이미지 교체
        diaryTabImage.sprite = diaryOnSprite;
        hintTabImage.sprite = hintOffSprite;
    }

    // '단서(힌트)' 영역을 눌렀을 때 (버튼 클릭용)
    public void ClickHintTab()
    {
        ForceClickHintTab();
    }

    // 단서 탭을 여는 실제 로직
    private void ForceClickHintTab()
    {
        // 1. 패널 전환
        diaryPanel.SetActive(false);
        hintPanel.SetActive(true);

        // 2. 탭 이미지 교체
        diaryTabImage.sprite = diaryOffSprite;
        hintTabImage.sprite = hintOnSprite;

        // 3. 단서 탭을 열 때마다 수집한 단서 목록을 최신화합니다.
        UpdateHintText();
    }

    // 수집한 단서를 각각의 텍스트 박스에 표시하는 기능
    private void UpdateHintText()
    {
        if (hintText_Radio != null)
        {
            hintText_Radio.gameObject.SetActive(PlayerPrefs.GetInt("Clue_Radio", 0) == 1);
            if (hintText_Radio.gameObject.activeSelf) hintText_Radio.text = "4";
        }

        if (hintText_Suit != null)
        {
            hintText_Suit.gameObject.SetActive(PlayerPrefs.GetInt("Clue_Suit", 0) == 1);
            if (hintText_Suit.gameObject.activeSelf) hintText_Suit.text = "2";
        }

        if (hintText_Toolbox != null)
        {
            hintText_Toolbox.gameObject.SetActive(PlayerPrefs.GetInt("Clue_Toolbox", 0) == 1);
            if (hintText_Toolbox.gameObject.activeSelf) hintText_Toolbox.text = "8";
        }

        if (hintText_Bookshelf != null)
        {
            hintText_Bookshelf.gameObject.SetActive(PlayerPrefs.GetInt("Clue_Bookshelf", 0) == 1);
            if (hintText_Bookshelf.gameObject.activeSelf) hintText_Bookshelf.text = "5";
        }
    }
}
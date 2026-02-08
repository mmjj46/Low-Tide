using UnityEngine;

/// <summary>
/// 게임 저장 데이터 클래스
/// JSON 직렬화를 통해 파일로 저장/로드됩니다.
/// </summary>
[System.Serializable]
public class SaveData
{
    [Header("게임 진행 상태")]
    public int day;                  // 현재 날짜 (1~15)
    public bool isMissionComplete;   // 오늘 미션 완료 여부

    [Header("플레이어 상태")]
    public Vector3 playerPos;        // 플레이어 위치

    [Header("기기 고장 상태")]
    // 각 오브젝트의 고장 여부 (true = 고장, false = 정상)
    public bool waterPurifierBroken;  // 정수기
    public bool foodDeviceBroken;     // 식량 장치
    public bool wallBroken;           // 벽
    public bool pipeBroken;           // 파이프
    public bool generatorBroken;      // 발전기
    public bool telescopeBroken;      // 망원경
    public bool communicateBroken;    // 통신 장치
    public bool lantonBroken;         // 랜턴

    /// <summary>
    /// GameManager에서 저장할 때 사용하는 생성자
    /// </summary>
    public SaveData(int _day, bool _isComplete, Vector3 _pos,
        bool _waterBroken, bool _foodBroken, bool _wallBroken, bool _pipeBroken,
        bool _generatorBroken, bool _telescopeBroken, bool _communicateBroken, bool _lantonBroken)
    {
        day = _day;
        isMissionComplete = _isComplete;
        playerPos = _pos;

        waterPurifierBroken = _waterBroken;
        foodDeviceBroken = _foodBroken;
        wallBroken = _wallBroken;
        pipeBroken = _pipeBroken;
        generatorBroken = _generatorBroken;
        telescopeBroken = _telescopeBroken;
        communicateBroken = _communicateBroken;
        lantonBroken = _lantonBroken;
    }

    /// <summary>
    /// JsonUtility 역직렬화(로드)를 위한 기본 생성자
    /// 모든 값을 초기 상태로 설정합니다.
    /// </summary>
    public SaveData()
    {
        day = 1;
        isMissionComplete = false;
        playerPos = Vector3.zero;

        waterPurifierBroken = false;
        foodDeviceBroken = false;
        wallBroken = false;
        pipeBroken = false;
        generatorBroken = false;
        telescopeBroken = false;
        communicateBroken = false;
        lantonBroken = false;
    }

    /// <summary>
    /// 디버깅용 문자열 출력
    /// </summary>
    public override string ToString()
    {
        return $"SaveData [Day {day}, Mission: {(isMissionComplete ? "Complete" : "Incomplete")}, Pos: {playerPos}]";
    }
}
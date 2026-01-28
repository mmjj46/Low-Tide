using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int day;                  // 현재 날짜
    public bool isMissionComplete;   // 오늘 미션 완료 여부
    public Vector3 playerPos;        // 플레이어 위치

    // ★ 각 오브젝트의 고장 상태 추가
    public bool waterPurifierBroken;
    public bool foodDeviceBroken;
    public bool wallBroken;
    public bool pipeBroken;
    public bool generatorBroken;
    public bool telescopeBroken;
    public bool communicateBroken;
    public bool lantonBroken;

    // 생성자
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

    // 기본 생성자 (JsonUtility 역직렬화용)
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
}
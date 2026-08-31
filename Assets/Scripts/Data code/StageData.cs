using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomWeight
{
    public RoomType type;
    public int weight;
}


[CreateAssetMenu(fileName = "New Stage", menuName = "Map/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 정보")]
    public string stageName;


    [Header("방 개수")]
    // 요구사항 "스테이지당 5~7개 맵" — 그대로 트리 깊이가 된다
    public int minRoomCount = 5;
    public int maxRoomCount = 7;


    [Header("방 타입 성향")]
    // 예: Battle 60 / Elite 15 / Merchant 15 / Treasure 10
    public List<RoomWeight> roomWeights = new List<RoomWeight>();

    // 한 경로(플레이어가 실제로 밟는 한 줄)에 등장할 수 있는 상인 수
    public int maxMerchantCount = 1;


    [Header("풀")]
    public List<RoomData> roomPool = new List<RoomData>();

    public List<EnemyData> enemyPool = new List<EnemyData>();


    [Header("보스")]
    // 보스방은 MapNode가 아니다 — 스테이지 카운트에 포함되지 않는 이유
    // 방 껍데기(RoomManager·바닥·문)와 보스를 따로 둔다.
    // 스테이지마다 방을 새로 만들지 않고 보스만 갈아 끼우기 위해서다.
    public GameObject bossRoomPrefab;

    public GameObject bossPrefab;


    public RoomData PickRoomPrefab(RoomType type, System.Random rng)
    {
        List<RoomData> candidates = roomPool.FindAll(r => r != null && r.type == type);

        if (candidates.Count == 0)
        {
            Debug.LogError($"[StageData] {name}: {type} 타입 방이 roomPool에 없습니다.");
            return null;
        }

        return candidates[rng.Next(candidates.Count)];
    }


    public List<EnemyData> GetEnemies(bool elite)
    {
        return enemyPool.FindAll(e => e != null && e.isElite == elite);
    }
}

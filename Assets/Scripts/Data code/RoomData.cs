using UnityEngine;

[CreateAssetMenu(fileName = "New Room", menuName = "Map/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("기본 정보")]
    public string roomName;

    public RoomType type;


    [Header("프리팹")]
    // 같은 타입의 배치 여러 개
    public GameObject[] prefabVariants;


    [Header("적 배치")]
    public int minEnemyCount = 2;
    public int maxEnemyCount = 4;


    // seed로 뽑아야 재방문 시 같은 방이 나온다
    public GameObject PickVariant(int seed)
    {
        if (prefabVariants == null || prefabVariants.Length == 0)
        {
            Debug.LogError($"[RoomData] {name}: prefabVariants가 비어 있습니다.");
            return null;
        }

        return prefabVariants[new System.Random(seed).Next(prefabVariants.Length)];
    }


    // 방에 스폰할 적 수도 노드 시드로 정한다 — 재방문 시 동일해야 하므로
    public int PickEnemyCount(int seed)
    {
        int min = Mathf.Max(0, minEnemyCount);
        int max = Mathf.Max(min, maxEnemyCount);

        return new System.Random(seed + 1).Next(min, max + 1);
    }
}

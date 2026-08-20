using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Reward")]
    [SerializeField, Min(0)] private int starDustPerKill;

    private RoomManager roomManager;
    private CurrencyWallet rewardWallet;

    private int enemyCount;


    // 적 종류·수·위치를 전부 node.seed로 정한다.
    // 되돌아갔다 다시 들어와도 같은 방이 나와야 하므로 Random.Range를 쓰면 안 된다.
    public void SpawnEnemies(RoomManager room, MapNode node, StageData stage, float power, CurrencyWallet wallet = null)
    {
        roomManager = room;
        rewardWallet = wallet;
        enemyCount = 0;

        if (node.room == null || stage == null)
        {
            Debug.LogError("[SpawnManager] RoomData 또는 StageData가 없습니다.");
            roomManager.RoomClear();
            return;
        }

        bool elite = node.type == RoomType.Elite;

        List<EnemyData> pool = stage.GetEnemies(elite);

        // 정예 풀이 비어 있으면 방이 통째로 비어 클리어돼 버린다. 일반 적으로 떨어뜨린다.
        if (pool.Count == 0 && elite)
        {
            Debug.LogWarning($"[SpawnManager] {stage.name}: 정예 적이 없어 일반 적으로 대체합니다.");
            pool = stage.GetEnemies(false);
        }

        if (pool.Count == 0)
        {
            Debug.LogError($"[SpawnManager] {stage.name}: enemyPool이 비어 있습니다.");
            roomManager.RoomClear();
            return;
        }

        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogError($"[SpawnManager] {name}: 스폰 지점이 없습니다.");
            roomManager.RoomClear();
            return;
        }


        System.Random rng = new System.Random(node.seed);

        // 스폰 지점보다 많이 뽑히면 겹쳐서 나온다
        int count = Mathf.Min(node.room.PickEnemyCount(node.seed), enemySpawnPoints.Length);

        // 지점 순서를 섞어야 같은 수라도 배치가 방마다 달라진다
        List<Transform> points = new List<Transform>(enemySpawnPoints);

        for (int i = points.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);

            Transform temp = points[i];
            points[i] = points[j];
            points[j] = temp;
        }


        for (int i = 0; i < count; i++)
        {
            EnemyData data = pool[rng.Next(pool.Count)];

            if (data == null || data.prefab == null)
            {
                Debug.LogError($"[SpawnManager] {stage.name}: EnemyData 또는 prefab이 비어 있습니다.");
                continue;
            }

            GameObject enemy = Instantiate(data.prefab, points[i].position, Quaternion.identity);

            // EnemyHP.Start()가 stats.currentHP를 읽으므로 Instantiate 직후에 주입해야 한다
            EnemyStats stats = enemy.GetComponent<EnemyStats>();

            if (stats != null)
                stats.Setup(data, power);

            EnemyHP enemyHP = enemy.GetComponent<EnemyHP>();

            if (enemyHP != null)
                enemyHP.SetSpawnManager(this);

            enemyCount++;
        }


        // 하나도 못 띄웠으면 문이 영영 안 열린다
        if (enemyCount <= 0)
            roomManager.RoomClear();
    }


    public void EnemyDead()
    {
        if (rewardWallet != null)
            rewardWallet.Add(starDustPerKill);

        enemyCount--;

        if (enemyCount <= 0)
        {
            roomManager.RoomClear();
        }
    }
}

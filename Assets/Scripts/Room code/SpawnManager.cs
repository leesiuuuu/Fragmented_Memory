using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Reward")]
    [SerializeField, Min(0)] private int starDustPerKill;

    private RoomManager roomManager;
    private CurrencyWallet rewardWallet;

    private int enemyCount;


    public void SpawnEnemies(RoomManager room, CurrencyWallet rewardWallet = null)
    {
        roomManager = room;
        this.rewardWallet = rewardWallet;

        enemyCount = enemySpawnPoints.Length;


        for (int i = 0; i < enemySpawnPoints.Length; i++)
{
    GameObject enemy = Instantiate(
        enemyPrefab,
        enemySpawnPoints[i].position,
        Quaternion.identity
    );


    EnemyHP enemyHP = enemy.GetComponent<EnemyHP>();

    if(enemyHP != null)
    {
        enemyHP.SetSpawnManager(this);
    }
}
    }


    public void EnemyDead()
    {
        if (rewardWallet != null)
            rewardWallet.Add(starDustPerKill);

        enemyCount--;

        if(enemyCount <= 0)
        {
            roomManager.RoomClear();
        }
    }
}

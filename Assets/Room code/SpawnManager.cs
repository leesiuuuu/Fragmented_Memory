using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] enemySpawnPoints;

    private RoomManager roomManager;

    private int enemyCount;


    public void SpawnEnemies(RoomManager room)
    {
        roomManager = room;

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
        enemyCount--;

        if(enemyCount <= 0)
        {
            roomManager.RoomClear();
        }
    }
}
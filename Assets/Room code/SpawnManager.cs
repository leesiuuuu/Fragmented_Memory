using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] enemySpawnPoints;

    private void Start()
    {
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        foreach (Transform point in enemySpawnPoints)
        {
            Instantiate(enemyPrefab, point.position, Quaternion.identity);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private List<GameObject> spawnedEnemys = new List<GameObject>();
    [SerializeField] private GameObject enemy;
    [SerializeField] private Vector2 box; // 생성 범위 (가로, 세로 크기)
    public void StartSpawn(int cnt)
    {
        StartCoroutine(spawnEnemy(cnt));
    }

    private IEnumerator spawnEnemy(int cnt)
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f));

        for (int i = 0; i < cnt; i++)
        {
            float randomX = Random.Range(-box.x / 2f, box.x / 2f);
            float randomY = Random.Range(-box.y / 2f, box.y / 2f);
            Vector2 spawnPos = new Vector2(transform.position.x + randomX, transform.position.y + randomY);

            var obj = Instantiate(enemy, spawnPos, Quaternion.identity);
            spawnedEnemys.Add(obj);

            float rand = Random.Range(1.5f, 3f);
            yield return new WaitForSeconds(rand);
        }
    }

    // 에디터 뷰에서 생성 범위를 시각적으로 확인하기 위한 코드
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(box.x, box.y, 0));
    }
}
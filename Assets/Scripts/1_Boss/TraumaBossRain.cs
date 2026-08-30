using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraumaBossRain : MonoBehaviour
{
    public GameObject[] fragmentPrefabs;
    public Transform centerPoint;

    public float moveSpeed = 8f;
    public float fallSpeed = 8f;
    public float fragmentSpacing = 1.2f;
    public float spawnHeight = 6f;

    public int rainCount = 3;
    public float rainDelay = 0.5f;

    private BossControl boss;
    private bool isAttacking;

    void Start()
    {
        boss = GetComponent<BossControl>();

        if (boss != null)
            boss.OnPatternSelected += Pattern;
    }

    void OnDestroy()
    {
        if (boss != null)
            boss.OnPatternSelected -= Pattern;
    }

    void Pattern(int patternID, float damage)
    {
        if (patternID == 3 && !isAttacking)
            StartCoroutine(Rain());
    }

    IEnumerator Rain()
    {
        isAttacking = true;

        if (centerPoint == null ||
            fragmentPrefabs == null ||
            fragmentPrefabs.Length == 0)
        {
            isAttacking = false;
            yield break;
        }

        while (Vector2.Distance(
            transform.position,
            centerPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                centerPoint.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        boss.PlayAttackMotion(3);

        for (int i = 0; i < rainCount; i++)
        {
            SpawnFragments();

            if (i < rainCount - 1)
                yield return new WaitForSeconds(rainDelay);
        }

        isAttacking = false;
    }

    void SpawnFragments()
    {
        int positionCount = 22;
        int fragmentCount = 11;

        float totalWidth =
            (positionCount - 1) * fragmentSpacing;

        float startX =
            transform.position.x - totalWidth / 2f;

        List<int> positions = new List<int>();

        for (int i = 0; i < positionCount; i++)
            positions.Add(i);

        for (int i = 0; i < fragmentCount; i++)
        {
            int randomIndex =
                Random.Range(0, positions.Count);

            int positionIndex =
                positions[randomIndex];

            positions.RemoveAt(randomIndex);

            float x =
                startX + positionIndex * fragmentSpacing;

            Vector3 spawnPosition = new Vector3(
                x,
                transform.position.y + spawnHeight,
                transform.position.z
            );

            GameObject selectedPrefab =
                fragmentPrefabs[
                    Random.Range(0, fragmentPrefabs.Length)
                ];

            GameObject fragment = Instantiate(
                selectedPrefab,
                spawnPosition,
                Quaternion.identity
            );

            TraumaFragment traumaFragment =
                fragment.GetComponent<TraumaFragment>();

            if (traumaFragment != null)
                traumaFragment.SetBoss(boss);

            Rigidbody2D rigid =
                fragment.GetComponent<Rigidbody2D>();

            if (rigid != null)
            {
                rigid.gravityScale = 0f;
                rigid.linearVelocity =
                    Vector2.down * fallSpeed;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (centerPoint == null)
            return;

        Gizmos.DrawWireSphere(
            centerPoint.position,
            0.2f
        );
    }
}
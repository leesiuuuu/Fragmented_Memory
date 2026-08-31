using System.Collections;
using UnityEngine;

public class BlackEyeWave : MonoBehaviour
{
    public GameObject wavePrefab;

    public float upDistance = 1.5f;
    public float upSpeed = 4f;
    public float waitTime = 0.5f;

    private BossControl boss;
    private bool isAttacking;

    void Awake()
    {
        boss = GetComponent<BossControl>();
    }

    void OnEnable()
    {
        if (boss == null)
            boss = GetComponent<BossControl>();

        if (boss != null)
            boss.OnPatternSelected += Pattern;
    }

    void OnDisable()
    {
        if (boss != null)
            boss.OnPatternSelected -= Pattern;
    }

    void Pattern(int patternID, float damage)
    {
        if (patternID == 2 && !isAttacking)
            StartCoroutine(Attack(damage));
    }

    IEnumerator Attack(float damage)
    {
        isAttacking = true;

        if (boss == null ||
            boss.Player == null ||
            wavePrefab == null)
        {
            isAttacking = false;
            yield break;
        }

        boss.SetCanMove(false);

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 upPosition = startPosition;
        upPosition.y += upDistance;

        boss.PlayAttackMotion(2);

        while (Vector2.Distance(
            transform.position,
            upPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                upPosition,
                upSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = upPosition;

        yield return new WaitForSeconds(waitTime);

        if (boss.Player == null)
        {
            transform.rotation = startRotation;
            boss.SetCanMove(true);
            isAttacking = false;
            yield break;
        }

        Vector2 direction =
            (boss.Player.position - transform.position).normalized;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        GameObject wave = Instantiate(
            wavePrefab,
            transform.position,
            Quaternion.identity
        );

        BlackEyeWaveProjectile projectile =
            wave.GetComponent<BlackEyeWaveProjectile>();

        if (projectile != null)
        {
            projectile.damage = damage;

            projectile.SetTarget(
                boss.Player,
                boss
            );
        }

        transform.rotation = startRotation;

        boss.SetCanMove(true);
        isAttacking = false;
    }
}
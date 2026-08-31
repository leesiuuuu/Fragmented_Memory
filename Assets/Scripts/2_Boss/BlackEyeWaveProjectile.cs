using UnityEngine;

public class BlackEyeWaveProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 5f;
    public float damage;

    private Transform target;
    private BossControl boss;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform player, BossControl bossControl)
    {
        target = player;
        boss = bossControl;
    }

    void Update()
    {
        if (boss != null && boss.IsDead())
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
            return;

        Vector3 direction =
            (target.position - transform.position).normalized;

        transform.position +=
            direction * speed * Time.deltaTime;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        float distance =
            Vector2.Distance(transform.position, target.position);

        if (distance <= 0.5f)
        {
            if (boss != null)
                boss.DamagePlayer(damage);

            Destroy(gameObject);
        }
    }
}

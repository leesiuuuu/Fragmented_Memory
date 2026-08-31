using UnityEngine;

public class BlackEyeMarkWave : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 3f;

    private Vector3 target;
    private GameObject mark;
    private BossControl boss;
    private float damage;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(
        Vector3 position,
        GameObject targetMark,
        BossControl bossControl,
        float attackDamage)
    {
        target = position;
        mark = targetMark;
        boss = bossControl;
        damage = attackDamage;
    }

    void Update()
    {
        if (boss != null && boss.IsDead())
        {
            if (mark != null)
                Destroy(mark);

            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target) <= 0.05f)
        {
            if (mark != null)
                Destroy(mark);

            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (boss != null)
            boss.DamagePlayer(damage);

        if (boss != null)
        {
            Debug.Log("파동 적중 : " + damage);
        }

        if (mark != null)
            Destroy(mark);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (mark != null)
            Destroy(mark);
    }
}
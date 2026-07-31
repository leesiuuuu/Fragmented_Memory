using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 1f;
    public float attackCoolTime = 2f;

    bool canAttack = true;

    PlayerHP playerHp;
    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHp = player.GetComponent<PlayerHP>();
    }

    void Update()
    {
        Check();
    }

    void Check()
    {
        float distance = Vector2.Distance(transform.position,
                                          player.position);

        if (distance <= attackRange && canAttack)
        {
            Attack();
        }
    }

    void Attack()
    {
        playerHp.TakeDamage(damage);

        canAttack = false;

        Invoke("ResetAttack", attackCoolTime);
    }

    void ResetAttack()
    {
        canAttack = true;
    }
}
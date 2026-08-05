using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private EnemyStats stats;

    public float attackRange = 1f;
    public float attackCoolTime = 2f;

    bool canAttack = true;

    PlayerHP playerHp;
    Transform player;


    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }


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
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );


        if(distance <= attackRange && canAttack)
        {
            Attack();
        }
    }


    void Attack()
    {
        playerHp.TakeDamage(stats.attack);

        canAttack = false;

        Invoke(nameof(ResetAttack), attackCoolTime);
    }


    void ResetAttack()
    {
        canAttack = true;
    }
}
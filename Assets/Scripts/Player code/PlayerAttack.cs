using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private PlayerStats playerStats;
    private SpriteRenderer spriteRenderer;

    private bool isAttack = false;
    private bool canAttack = true;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void Update()
    {
        AttackInput();
    }


    private void AttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isAttack && canAttack)
        {
            NormalAttack();
        }
    }


    private void NormalAttack()
    {
        isAttack = true;
        canAttack = false;

        animator.SetTrigger("Stroke");
    }


    public void Damage()
    {
        Vector2 attackPosition;


        // 바라보는 방향 판정
        if (spriteRenderer.flipX)
        {
            attackPosition = transform.position + Vector3.left * 1f;
        }
        else
        {
            attackPosition = transform.position + Vector3.right * 1f;
        }


        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            attackPosition,
            new Vector2(1.5f, 1f),
            0f
        );


        foreach (Collider2D enemy in enemies)
        {
            EnemyHP enemyHP = enemy.GetComponent<EnemyHP>();

            if (enemyHP != null)
            {
                int damage = playerStats.GetAttackDamage();

                enemyHP.TakeDamage(damage);

                Debug.Log($"Enemy Damage : {damage}");
            }
        }
    }


    public void EndAttack()
    {
        isAttack = false;
        canAttack = true;
    }


    private void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            return;


        Vector2 attackPosition;


        if (spriteRenderer.flipX)
        {
            attackPosition = transform.position + Vector3.left * 1f;
        }
        else
        {
            attackPosition = transform.position + Vector3.right * 1f;
        }


        Gizmos.DrawWireCube(
            attackPosition,
            new Vector3(1.5f, 1f, 0)
        );
    }
}
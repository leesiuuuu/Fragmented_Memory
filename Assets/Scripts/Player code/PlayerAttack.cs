using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float attackCoolTime = 0.5f;

    private bool canAttack = true;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void Update()
    {
        AttackInput();
    }


    private void AttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)
            && !combat.IsBusy
            && canAttack)
        {
            NormalAttack();
        }
    }


    private void NormalAttack()
    {
        combat.StartAction();

        canAttack = false;

        animator.SetTrigger("Stroke");
    }


    // 애니메이션

    public void Damage()
    {
        Vector2 attackPosition;

        if (spriteRenderer.flipX)
        {
            attackPosition = (Vector2)transform.position + Vector2.left;
        }
        else
        {
            attackPosition = (Vector2)transform.position + Vector2.right;
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


    // 애니메이션

    public void EndAttack()
    {
        combat.EndAction();

        Invoke(nameof(ResetAttackCoolTime), attackCoolTime);
    }


    private void ResetAttackCoolTime()
    {
        canAttack = true;
    }


    private void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            return;

        Vector2 attackPosition;

        if (spriteRenderer.flipX)
        {
            attackPosition = (Vector2)transform.position + Vector2.left;
        }
        else
        {
            attackPosition = (Vector2)transform.position + Vector2.right;
        }

        Gizmos.DrawWireCube(
            attackPosition,
            new Vector3(1.5f, 1f, 0f)
        );
    }
}
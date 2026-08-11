using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private PlayerHP playerHP;
    private SpriteRenderer spriteRenderer;

    [Header("Attack Box")]
    [SerializeField] private Transform attackBox;
    private BoxCollider2D attackCollider;

    [SerializeField] private float attackBoxOffset = 1f;


    [Header("Attack")]
    [SerializeField] private float attackCoolTime = 0.5f;


    private int attackCombo = 0;
    private bool comboQueued = false;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool isComboWindowOpen = false;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        playerHP = GetComponent<PlayerHP>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        attackCollider = attackBox.GetComponent<BoxCollider2D>();
    }



    private void Update()
    {
        if(playerHP.IsDead || GameplayInputLock.IsLocked)
            return;


        UpdateAttackBoxDirection();

        AttackInput();
    }



    private void UpdateAttackBoxDirection()
    {
        if(spriteRenderer.flipX)
        {
            attackBox.localPosition =
                new Vector2(
                    -attackBoxOffset,
                    attackBox.localPosition.y
                );
        }
        else
        {
            attackBox.localPosition =
                new Vector2(
                    attackBoxOffset,
                    attackBox.localPosition.y
                );
        }
    }



    private void AttackInput()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(!combat.IsBusy && canAttack)
            {
                StartAttack();
            }
            else if(isAttacking && isComboWindowOpen)
            {
                comboQueued = true;
            }
        }
    }



    private void StartAttack()
    {
        combat.StartAction();

        canAttack = false;

        comboQueued = false;
        isAttacking = true;
        isComboWindowOpen = false;

        attackCombo = 1;


        animator.SetInteger(
            "AttackCombo",
            attackCombo
        );

        animator.SetTrigger("Attack");
    }



    // Animation Event
    public void Damage()
    {
        if(playerHP.IsDead)
            return;


        Collider2D[] enemies =
            Physics2D.OverlapBoxAll(
                attackCollider.bounds.center,
                attackCollider.bounds.size,
                0f
            );

        HashSet<EnemyHP> hitEnemies = new HashSet<EnemyHP>();

        foreach(Collider2D enemy in enemies)
        {
            EnemyHP enemyHP =
                enemy.GetComponentInParent<EnemyHP>();


            if(enemyHP != null && hitEnemies.Add(enemyHP))
            {
                int damage =
                    Mathf.RoundToInt(
                        playerStats.GetAttackDamage() * 0.5f
                    );


                enemyHP.TakeDamage(damage);
            }
        }
    }



    // Animation Event
    public void OpenComboWindow()
    {
        if (isAttacking)
            isComboWindowOpen = true;
    }



    // Animation Event
    public void CheckCombo()
    {
        isComboWindowOpen = false;

        if(comboQueued)
        {
            comboQueued = false;

            attackCombo = Mathf.Min(attackCombo + 1, 3);


            animator.SetInteger(
                "AttackCombo",
                attackCombo
            );
        }
    }



    // Animation Event
    public void EndAttack()
    {
        combat.EndAction();


        comboQueued = false;
        isAttacking = false;
        isComboWindowOpen = false;


        attackCombo = 0;


        animator.SetInteger(
            "AttackCombo",
            0
        );


        Invoke(
            nameof(ResetAttackCoolTime),
            attackCoolTime
        );
    }



    private void ResetAttackCoolTime()
    {
        canAttack = true;
    }



    private void OnDisable()
    {
        CancelInvoke(nameof(ResetAttackCoolTime));

        if (isAttacking)
            combat?.EndAction();

        comboQueued = false;
        isAttacking = false;
        isComboWindowOpen = false;
        attackCombo = 0;
        canAttack = true;
    }



    private void OnDrawGizmosSelected()
    {
        if(attackBox == null)
            return;


        BoxCollider2D collider =
            attackBox.GetComponent<BoxCollider2D>();


        if(collider == null)
            return;


        Gizmos.DrawWireCube(
            attackBox.position,
            collider.size
        );
    }
}

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
        if(playerHP.IsDead)
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
            Debug.Log(
                "Q 입력 / Busy : "
                + combat.IsBusy
                + " Cool : "
                + canAttack
            );


            if(!combat.IsBusy && canAttack)
            {
                StartAttack();
            }
            else if(combat.IsBusy)
            {
                comboQueued = true;

                Debug.Log("콤보 저장");
            }
        }
    }



    private void StartAttack()
    {
        Debug.Log("공격 시작");


        combat.StartAction();


        canAttack = false;


        comboQueued = false;


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
        Debug.Log("Damage Event 실행");


        if(playerHP.IsDead)
            return;


        Collider2D[] enemies =
            Physics2D.OverlapBoxAll(
                attackBox.position,
                attackCollider.size,
                0f
            );


        foreach(Collider2D enemy in enemies)
        {
            EnemyHP enemyHP =
                enemy.GetComponent<EnemyHP>();


            if(enemyHP != null)
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
    public void CheckCombo()
    {
        Debug.Log(
            "CheckCombo / Queue : "
            + comboQueued
            + " Combo : "
            + attackCombo
        );

        if(comboQueued)
        {
            comboQueued = false;

            attackCombo++;

            Debug.Log(
                "AttackCombo 변경 : "
                + attackCombo
            );

            animator.SetInteger(
                "AttackCombo",
                attackCombo
            );

            Debug.Log(
                "Animator 값 : "
                + animator.GetInteger("AttackCombo")
            );
        }
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

Debug.Log(info.shortNameHash);
Debug.Log(info.IsName("FirstStroke"));
Debug.Log(info.IsName("SecondStroke"));
Debug.Log(info.IsName("ThirdStroke"));
    }



    // Animation Event
    public void EndAttack()
    {
        Debug.Log("EndAttack 실행");


        combat.EndAction();


        comboQueued = false;


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
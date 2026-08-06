using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private SpriteRenderer spriteRenderer;
    private PlayerHP playerHP;


    [SerializeField] private float pokeCoolTime = 2f;
    [SerializeField] private float strikeCoolTime = 3f;


    private bool canPoke = true;
    private bool canStrike = true;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHP = GetComponent<PlayerHP>();
    }



    private void Update()
    {
        if(playerHP.IsDead)
            return;

        SkillInput();
    }



    private void SkillInput()
    {
        if(Input.GetKeyDown(KeyCode.R)
            && !combat.IsBusy
            && canPoke)
        {
            Poke();
        }


        if(Input.GetKeyDown(KeyCode.F)
            && !combat.IsBusy
            && canStrike)
        {
            Strike();
        }
    }



    private void Poke()
    {
        combat.StartAction();

        canPoke = false;

        animator.SetTrigger("Poke");

        Invoke(
            nameof(ResetPokeCoolTime),
            pokeCoolTime
        );
    }



    private void Strike()
    {
        combat.StartAction();

        canStrike = false;

        animator.SetTrigger("Strike");

        Invoke(
            nameof(ResetStrikeCoolTime),
            strikeCoolTime
        );
    }



    // Animation Event

    public void PokeDamage()
    {
        AttackDamage(1.25f);
    }



    // Animation Event

    public void StrikeDamage()
    {
        AttackDamage(2f);
    }



    private void AttackDamage(float damageMultiplier)
    {
        if(playerHP.IsDead)
            return;


        Vector2 attackPosition;


        if(spriteRenderer.flipX)
        {
            attackPosition =
                (Vector2)transform.position + Vector2.left;
        }
        else
        {
            attackPosition =
                (Vector2)transform.position + Vector2.right;
        }



        Collider2D[] enemies =
            Physics2D.OverlapBoxAll(
                attackPosition,
                new Vector2(1.5f,1f),
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
                        playerStats.GetAttackDamage()
                        * damageMultiplier
                    );


                enemyHP.TakeDamage(damage);
            }
        }
    }



    // Animation Event

    public void EndSkill()
    {
        combat.EndAction();
    }



    private void ResetPokeCoolTime()
    {
        canPoke = true;
    }



    private void ResetStrikeCoolTime()
    {
        canStrike = true;
    }
}
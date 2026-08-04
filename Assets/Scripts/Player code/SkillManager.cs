using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Transform hpBar;

    [SerializeField] private float pokeCoolTime = 2f;
    [SerializeField] private float strikeCoolTime = 3f;

    private bool canPoke = true;
    private bool canStrike = true;

    private Vector3 hpBarOriginPos;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (hpBar != null)
        {
            hpBarOriginPos = hpBar.localPosition;
        }
    }


    private void Update()
    {
        SkillInput();
    }


    private void SkillInput()
    {
        // 찌르기
        if (Input.GetKeyDown(KeyCode.R)
            && !combat.IsBusy
            && canPoke)
        {
            Poke();
        }

        // 내려찍기
        if (Input.GetKeyDown(KeyCode.F)
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

        Invoke(nameof(ResetPokeCoolTime), pokeCoolTime);
    }


    private void Strike()
    {
        combat.StartAction();

        canStrike = false;

        animator.SetTrigger("Strike");

        Invoke(nameof(ResetStrikeCoolTime), strikeCoolTime);
    }


    // 애니메이션

    public void PokeDamage()
    {
        AttackDamage(1.25f);
    }


    // 애니메이션

    public void StrikeDamage()
    {
        AttackDamage(2f);
    }


    private void AttackDamage(float damageMultiplier)
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
                int damage = Mathf.RoundToInt(
                    playerStats.GetAttackDamage() * damageMultiplier
                );

                enemyHP.TakeDamage(damage);

                Debug.Log($"Skill Damage : {damage}");
            }
        }
    }


    public void HPBarUp()
    {
        if (hpBar != null)
        {
            hpBar.localPosition = hpBarOriginPos + new Vector3(0f, 2f, 0f);
        }
    }


    public void HPBarReset()
    {
        if (hpBar != null)
        {
            hpBar.localPosition = hpBarOriginPos;
        }
    }


    // 애니메이션

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
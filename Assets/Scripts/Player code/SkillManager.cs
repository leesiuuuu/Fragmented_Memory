using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private const float StrikeRiseSpeed = 35f;
    private const float StrikeFallSpeed = 30f;
    private const float StrikeForwardSpeed = 4f;

    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private SpriteRenderer spriteRenderer;
    private PlayerHP playerHP;
    private Rigidbody2D rigid;
    private PlayerMovement movement;


    [SerializeField] private float pokeCoolTime = 2f;
    [SerializeField] private float strikeCoolTime = 3f;


    private bool canPoke = true;
    private bool canStrike = true;
    private readonly HashSet<EnemyHP> hitEnemies = new HashSet<EnemyHP>();



    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHP = GetComponent<PlayerHP>();
        rigid = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }



    private void Update()
    {
        if(playerHP.IsDead || GameplayInputLock.IsLocked)
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
        if(!playerHP.IsDead && EffectManager.Instance != null)
        {
            Vector3 effectPosition =
                transform.position
                + (spriteRenderer.flipX ? Vector3.left : Vector3.right);
            Quaternion effectRotation = spriteRenderer.flipX
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;

            EffectManager.Instance.Play(
                EffectId.Poke,
                effectPosition,
                effectRotation
            );
        }

        AttackDamage(1.25f);
    }



    // Animation Event

    public void StrikeRise()
    {
        rigid.linearVelocity = new Vector2(
            rigid.linearVelocity.x,
            StrikeRiseSpeed
        );
    }



    // Animation Event

    public void StrikeHover()
    {
        rigid.linearVelocity = new Vector2(
            rigid.linearVelocity.x,
            0f
        );
    }



    // Animation Event

    public void StrikeFall()
    {
        float direction = spriteRenderer.flipX ? -1f : 1f;

        movement.SetForcedHorizontalSpeed(
            direction * StrikeForwardSpeed
        );

        rigid.linearVelocity = new Vector2(
            rigid.linearVelocity.x,
            -StrikeFallSpeed
        );
    }



    // Animation Event

    public void StrikeDamage()
    {
        movement.ClearForcedHorizontalSpeed();

        if(!playerHP.IsDead && EffectManager.Instance != null)
        {
            Vector3 effectPosition =
                transform.position
                + (spriteRenderer.flipX ? Vector3.left : Vector3.right);
            Quaternion effectRotation = spriteRenderer.flipX
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;

            EffectManager.Instance.Play(
                EffectId.Strike,
                effectPosition,
                effectRotation
            );
        }

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



        hitEnemies.Clear();

        foreach(Collider2D enemy in enemies)
        {
            EnemyHP enemyHP =
                enemy.GetComponentInParent<EnemyHP>();


            if(enemyHP != null && hitEnemies.Add(enemyHP))
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

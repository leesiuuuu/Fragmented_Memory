using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    private EnemyStats stats;
    private Animator animator;

    private static readonly int Death = Animator.StringToHash("Death");

    private SpawnManager spawnManager;

    [SerializeField] private HPBar hpBar;
    [SerializeField] private AnimationClip deathAnimation;
    [SerializeField] private float deathDuration = 2f;
    [SerializeField] private bool hideHPBarOnDeath;
    private bool isDead;


    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        animator = GetComponent<Animator>();
    }


    private void Start()
    {
        hpBar.SetHP(stats.currentHP, stats.maxHP);
    }


    public void SetSpawnManager(SpawnManager manager)
    {
        spawnManager = manager;
    }


    public int TakeDamage(int rawDamage)
    {
        if (isDead)
            return 0;

        int finalDamage = DamageCalculator.Calculate(
            rawDamage,
            stats.defense);
        int effectiveDamage = Mathf.Min(finalDamage, stats.currentHP);

        stats.currentHP -= effectiveDamage;

        // if (effectiveDamage > 0 && EffectManager.Instance != null)
        // {
        //     EffectManager.Instance.Play(
        //         EffectId.EnemyHit,
        //         transform.position,
        //         Quaternion.identity
        //     );
        // }


        hpBar.SetHP(stats.currentHP, stats.maxHP);


        if (stats.currentHP <= 0)
        {
            Die();
        }

        return effectiveDamage;
    }


    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (hideHPBarOnDeath && hpBar != null)
            hpBar.gameObject.SetActive(false);

        if(spawnManager != null)
        {
            spawnManager.EnemyDead();
        }

        EnemyDeathExplosion deathExplosion = GetComponent<EnemyDeathExplosion>();

        if (deathAnimation == null)
        {
            if (deathExplosion != null)
                deathExplosion.Explode();

            Destroy(gameObject);
            return;
        }

        EnemyMovement movement = GetComponent<EnemyMovement>();
        EnemyAttack attack = GetComponent<EnemyAttack>();
        Rigidbody2D rigid = GetComponent<Rigidbody2D>();

        if (movement != null)
            movement.enabled = false;

        if (attack != null)
            attack.enabled = false;

        if (rigid != null)
            rigid.linearVelocity = Vector2.zero;

        animator.SetTrigger(Death);
        float duration = deathDuration > 0f
            ? deathDuration
            : deathAnimation.length;
        animator.speed = Mathf.Max(deathAnimation.length / duration, 0.01f);

        Invoke(nameof(FinishDeath), duration);
    }


    private void FinishDeath()
    {
        EnemyDeathExplosion deathExplosion = GetComponent<EnemyDeathExplosion>();

        if (deathExplosion != null)
            deathExplosion.Explode();

        Destroy(gameObject);
    }
}

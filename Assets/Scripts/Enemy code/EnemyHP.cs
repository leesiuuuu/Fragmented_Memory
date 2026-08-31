using System.Collections;
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
    private bool isBoss;
    private Coroutine burnRoutine;

    public bool IsDead => isDead;
    public int CurrentHP => stats != null ? stats.currentHP : 0;
    public int MaxHP => stats != null ? stats.maxHP : 0;

    // 보스는 SpawnManager를 타지 않아 EnemyDead()가 불리지 않는다.
    // 방이 보스 사망을 직접 알 수 있도록 열어 둔다.
    public System.Action OnDeath;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (hpBar != null && stats != null)
            hpBar.SetHP(stats.currentHP, stats.maxHP);
    }

    public void SetBoss()
    {
        isBoss = true;
    }

    public void SetSpawnManager(SpawnManager manager)
    {
        spawnManager = manager;
    }

    public void ApplyBurn(int damagePerTick, float duration)
    {
        if (isDead)
            return;

        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = StartCoroutine(Burn(damagePerTick, duration));
    }

    private IEnumerator Burn(int damagePerTick, float duration)
    {
        float elapsed = 0f;

        while (!isDead && elapsed < duration)
        {
            yield return new WaitForSeconds(0.5f);

            elapsed += 0.5f;
            TakeDamage(damagePerTick, true);
        }

        burnRoutine = null;
    }

    public int TakeDamage(int rawDamage)
    {
        return TakeDamage(rawDamage, false);
    }

    public int TakeDamage(int rawDamage, bool ignoreDefense)
    {
        if (isDead || stats == null)
            return 0;

        int finalDamage = DamageCalculator.Calculate(
            rawDamage,
            stats.defense,
            ignoreDefense
        );

        int effectiveDamage = Mathf.Min(
            finalDamage,
            stats.currentHP
        );

        stats.currentHP -= effectiveDamage;

        if (hpBar != null)
            hpBar.SetHP(stats.currentHP, stats.maxHP);

        if (stats.currentHP <= 0)
            Die();

        return effectiveDamage;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (isBoss)
        {
            OnDeath?.Invoke();
            return;
        }

        GetComponent<EnemyMemoryDrop>()?.TryDrop();

        if (hideHPBarOnDeath && hpBar != null)
            hpBar.gameObject.SetActive(false);

        if (spawnManager != null)
            spawnManager.EnemyDead();

        EnemyDeathExplosion deathExplosion =
            GetComponent<EnemyDeathExplosion>();

        if (deathAnimation == null)
        {
            if (deathExplosion != null)
                deathExplosion.Explode();

            Destroy(gameObject);
            return;
        }

        EnemyMovement movement =
            GetComponent<EnemyMovement>();

        EnemyAttack attack =
            GetComponent<EnemyAttack>();

        Rigidbody2D rigid =
            GetComponent<Rigidbody2D>();

        if (movement != null)
            movement.enabled = false;

        if (attack != null)
            attack.enabled = false;

        if (rigid != null)
        {
            rigid.linearVelocity = Vector2.zero;
            rigid.simulated = false;
        }

        if (animator != null)
            animator.SetTrigger(Death);

        float duration = deathDuration > 0f
            ? deathDuration
            : deathAnimation.length;

        if (animator != null)
        {
            animator.speed =
                Mathf.Max(
                    deathAnimation.length / duration,
                    0.01f
                );
        }

        Invoke(nameof(FinishDeath), duration);
    }

    private void FinishDeath()
    {
        EnemyDeathExplosion deathExplosion =
            GetComponent<EnemyDeathExplosion>();

        if (deathExplosion != null)
            deathExplosion.Explode();

        Destroy(gameObject);
    }
}
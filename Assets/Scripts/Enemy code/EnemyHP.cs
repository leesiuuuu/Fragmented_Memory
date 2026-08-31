using System.Collections;
using UnityEngine;

// EnemyStats의 현재 체력을 갱신한다. 체력이 끝나면 드롭과 방 진행과 사망 연출을 처리한다.
// 사망 연출 중에는 이동과 공격과 물리 충돌을 멈춘다. 연출이 끝나면 적 오브젝트를 제거한다.
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
    private Coroutine burnRoutine;

    // 보스는 SpawnManager를 타지 않아 EnemyDead()가 불리지 않는다.
    // 방이 보스 사망을 직접 알 수 있도록 열어 둔다.
    public event System.Action Died;


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
        if (isDead)
            return 0;

        int finalDamage = DamageCalculator.Calculate(
            rawDamage,
            stats.defense,
            ignoreDefense);
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

        Died?.Invoke();

        GetComponent<EnemyMemoryDrop>()?.TryDrop();

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
        {
            rigid.linearVelocity = Vector2.zero;
            rigid.simulated = false;
        }

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

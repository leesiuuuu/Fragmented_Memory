using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    private PlayerStats stats;
    private Animator animator;
    private PlayerInvincibility invincibility;
    private ParryManager parryManager;

    [SerializeField] private HPBar hpBar;

    private bool isDead;
    public bool IsDead => isDead;
    public event System.Action<int, int> HealthChanged;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
        invincibility = GetComponent<PlayerInvincibility>();
        parryManager = GetComponent<ParryManager>();

        if (stats != null)
            stats.StatsChanged += UpdateHPBar;
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.StatsChanged -= UpdateHPBar;
    }

    private void Start()
    {
        hpBar.SetHP(stats.currentHealth, stats.maxHealth);
    }

    public int TakeDamage(int rawDamage)
    {
        if (isDead || (invincibility != null && invincibility.IsInvincible))
            return 0;

        if (parryManager != null && parryManager.TryParry())
            return 0;

        int finalDamage = DamageCalculator.Calculate(
            rawDamage,
            stats.defense);
        int effectiveDamage = Mathf.Min(finalDamage, stats.currentHealth);

        stats.currentHealth -= effectiveDamage;

        if(effectiveDamage > 0 && invincibility != null)
            invincibility.StartHitInvincibility();

        hpBar.SetHP(stats.currentHealth, stats.maxHealth);
        HealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);

        if (stats.currentHealth <= 0)
        {
            Die();
        }

        return effectiveDamage;
    }

    public void Heal(int amount)
    {
        stats.currentHealth += amount;

        if (stats.currentHealth > stats.maxHealth)
            stats.currentHealth = stats.maxHealth;

        hpBar.SetHP(stats.currentHealth, stats.maxHealth);
        HealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Debug.Log("플레이어 사망");
        animator.SetTrigger("Death");

        // TODO
        // 이동 정지
        // 사망 애니메이션
        // 게임 오버
    }

    private void UpdateHPBar()
    {
        if (hpBar != null && stats != null)
            hpBar.SetHP(stats.currentHealth, stats.maxHealth);
    }
}

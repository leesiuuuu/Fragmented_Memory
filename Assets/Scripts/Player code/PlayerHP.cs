using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    private PlayerStats stats;
    private Animator animator;

    [SerializeField] private HPBar hpBar;

    private bool isDead;
    public bool IsDead => isDead;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();

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
        if (isDead)
            return 0;

        int finalDamage = DamageCalculator.Calculate(
            rawDamage,
            stats.defense);
        int effectiveDamage = Mathf.Min(finalDamage, stats.currentHealth);

        stats.currentHealth -= effectiveDamage;

        hpBar.SetHP(stats.currentHealth, stats.maxHealth);

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
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("플레이어 사망");
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

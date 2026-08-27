using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    private PlayerStats stats;
    private Animator animator;
    private PlayerInvincibility invincibility;
    private ParryManager parryManager;
    private ItemInventory itemInventory;
    private PlayerSynergyManager synergyManager;

    [SerializeField] private HPBar hpBar;

    private bool isDead;
    public bool IsDead => isDead;
    public event System.Action<int, int> HealthChanged;
    public event System.Action Died;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
        invincibility = GetComponent<PlayerInvincibility>();
        parryManager = GetComponent<ParryManager>();
        itemInventory = GetComponent<ItemInventory>();
        synergyManager = GetComponent<PlayerSynergyManager>();

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

        int finalDamage = synergyManager != null && synergyManager.IsDespairInstantDeath
            ? stats.currentHealth
            : DamageCalculator.Calculate(rawDamage, stats.CurrentDefense);
        int effectiveDamage = Mathf.Min(finalDamage, stats.currentHealth);

        stats.currentHealth -= effectiveDamage;
        synergyManager?.OnPlayerDamaged();

        if(effectiveDamage > 0 && invincibility != null)
            invincibility.StartHitInvincibility();

        hpBar.SetHP(stats.currentHealth, stats.maxHealth);
        HealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);

        if (stats.currentHealth <= 0)
        {
            if (TryRevive())
                return effectiveDamage;

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

        animator.SetTrigger("Death");
        Died?.Invoke();
    }

    public void RestoreAfterRun()
    {
        isDead = false;
        stats.currentHealth = stats.maxHealth;
        hpBar.SetHP(stats.currentHealth, stats.maxHealth);
        HealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);
    }

    private bool TryRevive()
    {
        if (itemInventory == null)
            return false;

        foreach (ItemInventory.Entry entry in itemInventory.Items)
        {
            if (entry.item != null && entry.item.effectType == ItemEffectType.Revival)
            {
                ItemData revivalItem = entry.item;
                itemInventory.Consume(revivalItem);
                stats.currentHealth = Mathf.Max(1, Mathf.RoundToInt(stats.maxHealth * revivalItem.effectValue / 100f));
                hpBar.SetHP(stats.currentHealth, stats.maxHealth);
                HealthChanged?.Invoke(stats.currentHealth, stats.maxHealth);
                return true;
            }
        }

        return false;
    }

    private void UpdateHPBar()
    {
        if (hpBar != null && stats != null)
            hpBar.SetHP(stats.currentHealth, stats.maxHealth);
    }
}

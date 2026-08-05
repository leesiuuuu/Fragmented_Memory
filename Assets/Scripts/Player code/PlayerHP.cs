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
    }

    private void Start()
    {
        hpBar.SetHP(stats.currentHealth, stats.maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        damage -= stats.defense;

        if (damage < 1)
            damage = 1;

        stats.currentHealth -= damage;

        if (stats.currentHealth < 0)
            stats.currentHealth = 0;

        hpBar.SetHP(stats.currentHealth, stats.maxHealth);

        if (stats.currentHealth <= 0)
        {
            Die();
        }
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
}
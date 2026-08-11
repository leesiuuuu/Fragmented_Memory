using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("기본 스탯")]
    public int maxHealth = 1500;
    public int attack = 300;
    public int defense = 0;

    [Range(0f, 100f)]
    public float criticalChance = 5f;

    public float criticalDamage = 150f;

    [Range(0f, 100f)]
    public float lifeSteal = 5f;

    public int charm = 15;
    public int maxMemoryCount = 8;

    [Header("현재 스탯")]
    public int currentHealth;
    public event System.Action StatsChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void ApplyStat(StatData stat)
    {
        int previousMaxHealth = maxHealth;

        maxHealth = Mathf.Max(1, maxHealth + stat.health);
        attack += stat.attack;
        defense += stat.defense;

        criticalChance += stat.criticalChance;
        criticalDamage += stat.criticalDamage;
        lifeSteal += stat.lifeSteal;
        charm += stat.charm;
 

        int increasedHealth = Mathf.Max(0, maxHealth - previousMaxHealth);
        currentHealth = Mathf.Clamp(
            currentHealth + increasedHealth,
            0,
            maxHealth);

        StatsChanged?.Invoke();
    }

    public void RemoveStat(StatData stat)
    {
        maxHealth = Mathf.Max(1, maxHealth - stat.health);
        attack -= stat.attack;
        defense -= stat.defense;

        criticalChance -= stat.criticalChance;
        criticalDamage -= stat.criticalDamage;
        lifeSteal -= stat.lifeSteal;
        charm -= stat.charm;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        StatsChanged?.Invoke();
    }

    public int GetAttackDamage()
    {
        int damage = attack;

        if (Random.Range(0f, 100f) <= criticalChance)
        {
            damage = Mathf.RoundToInt(
                attack * (criticalDamage / 100f));

            Debug.Log("Critical!");
        }

        return damage;
    }
}

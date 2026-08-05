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


    [Header("Current")]
    public int currentHealth;


    private void Awake()
    {
        currentHealth = maxHealth;
    }


    public void ApplyStat(MemoryData memory)
    {
        maxHealth += (int)memory.health;
        attack += (int)memory.attack;
        defense += (int)memory.defense;

        criticalChance += memory.criticalChance;
        criticalDamage += memory.criticalDamage;
        lifeSteal += memory.lifeSteal;
        charm += (int)memory.charm;

        currentHealth = maxHealth;
    }


    public void RemoveStat(StatData stat)
    {
        maxHealth -= stat.health;
        attack -= stat.attack;
        defense -= stat.defense;

        criticalChance -= stat.criticalChance;
        criticalDamage -= stat.criticalDamage;

        lifeSteal -= stat.lifeSteal;

        charm -= stat.charm;


        if(currentHealth > maxHealth)
            currentHealth = maxHealth;
    }


    public int GetAttackDamage()
    {
        int damage = attack;


        if(Random.Range(0f,100f) <= criticalChance)
        {
            damage = Mathf.RoundToInt(
                attack * (criticalDamage / 100f)
            );

            Debug.Log("Critical!");
        }


        return damage;
    }


    public void TakeDamage(int damage)
    {
        damage -= defense;


        if(damage < 1)
            damage = 1;


        currentHealth -= damage;


        if(currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }


    public void Heal(int amount)
    {
        currentHealth += amount;


        if(currentHealth > maxHealth)
            currentHealth = maxHealth;
    }


    private void Die()
    {
        Debug.Log("Player Dead");
    }
}
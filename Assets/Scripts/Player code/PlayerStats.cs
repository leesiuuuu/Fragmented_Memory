using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private int temporaryAttack;
    private float temporaryLifeSteal;
    private float temporaryCriticalChance;
    private int synergyAttack;
    private int synergyDefense;
    private int synergyCharm;
    private float synergyCriticalChance;
    private float synergyCriticalDamage;
    private float synergyLifeSteal;
    private Coroutine criticalChanceRoutine;

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
    public int maxMemoryCount = 18;

    [Header("현재 스탯")]
    public int currentHealth;
    public event System.Action StatsChanged;
    public int CurrentAttack => attack + temporaryAttack + synergyAttack;
    public int CurrentDefense => defense + synergyDefense;
    public int CurrentCharm => charm + synergyCharm;
    public float CurrentLifeSteal => lifeSteal + temporaryLifeSteal + synergyLifeSteal;
    public float CurrentCriticalChance => criticalChance + temporaryCriticalChance + synergyCriticalChance;
    public float CurrentCriticalDamage => criticalDamage + synergyCriticalDamage;

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
        int damage = CurrentAttack;

        if (Random.Range(0f, 100f) <= CurrentCriticalChance)
        {
            damage = Mathf.RoundToInt(
                CurrentAttack * (CurrentCriticalDamage / 100f));

            // Debug.Log("Critical!");
        }

        return damage;
    }

    public void SetTemporaryAttack(int amount)
    {
        temporaryAttack = amount;
        StatsChanged?.Invoke();
    }

    public void SetTemporaryLifeSteal(float amount)
    {
        temporaryLifeSteal = amount;
        StatsChanged?.Invoke();
    }

    public void ApplyTemporaryCriticalChance(float amount, float duration)
    {
        if (criticalChanceRoutine != null)
            StopCoroutine(criticalChanceRoutine);

        criticalChanceRoutine = StartCoroutine(TemporaryCriticalChance(amount, duration));
    }

    public void SetSynergyStats(int attackBonus, int defenseBonus, int charmBonus,
        float criticalChanceBonus, float criticalDamageBonus, float lifeStealBonus)
    {
        synergyAttack = attackBonus;
        synergyDefense = defenseBonus;
        synergyCharm = charmBonus;
        synergyCriticalChance = criticalChanceBonus;
        synergyCriticalDamage = criticalDamageBonus;
        synergyLifeSteal = lifeStealBonus;
        StatsChanged?.Invoke();
    }

    private IEnumerator TemporaryCriticalChance(float amount, float duration)
    {
        temporaryCriticalChance = amount;
        StatsChanged?.Invoke();

        yield return new WaitForSeconds(duration);

        temporaryCriticalChance = 0f;
        criticalChanceRoutine = null;
        StatsChanged?.Invoke();
    }
}

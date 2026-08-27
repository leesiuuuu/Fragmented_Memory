using System.Collections.Generic;
using UnityEngine;

public class PlayerSynergyManager : MonoBehaviour
{
    private Inventory inventory;
    private PlayerStats stats;
    private PlayerHP playerHP;
    private readonly Dictionary<SynergyType, int> counts = new Dictionary<SynergyType, int>();
    private float lastAttackTime;
    private float defenseTickTime;
    private int roseCharm;
    private int despairAttack;
    private int gamblerAttack;
    private int escapeDefense;

    public bool IsDespairInstantDeath => Count(SynergyType.Despair) >= 3;
    public event System.Action Changed;

    public int GetCount(SynergyType type) => Count(type);

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        stats = GetComponent<PlayerStats>();
        playerHP = GetComponent<PlayerHP>();
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.Changed += Recalculate;
        if (playerHP != null)
            playerHP.HealthChanged += HandleHealthChanged;
    }

    private void Start() => Recalculate();

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Changed -= Recalculate;
        if (playerHP != null)
            playerHP.HealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        if (Count(SynergyType.Escape) == 0 || Time.time < defenseTickTime)
            return;

        defenseTickTime = Time.time + 1f;
        if (Time.time - lastAttackTime >= 1f)
        {
            int max = Count(SynergyType.Escape) >= 2 ? 150 : 50;
            escapeDefense = Mathf.Min(escapeDefense + 25, max);
            ApplyStats();
        }
    }

    public void OnDamageDealt(EnemyHP enemy, int damage)
    {
        if (damage <= 0)
            return;

        lastAttackTime = Time.time;
        escapeDefense = 0;

        int overcome = Count(SynergyType.OvercomeTrauma);
        if (overcome > 0)
        {
            float percent = overcome >= 3 ? 35f : overcome >= 2 ? 25f : 10f;
            playerHP.Heal(Mathf.RoundToInt(damage * percent / 100f));
        }

        int rose = Count(SynergyType.Rose);
        if (rose > 0)
        {
            int amount = rose >= 3 ? 6 : rose >= 2 ? 4 : 2;
            int max = rose >= 3 ? 150 : rose >= 2 ? 100 : 50;
            roseCharm = Mathf.Min(roseCharm + amount, max);
        }

        int despair = Count(SynergyType.Despair);
        if (despair > 0)
        {
            int amount = despair >= 3 ? 200 : despair >= 2 ? 150 : 100;
            int max = despair >= 3 ? 1800 : despair >= 2 ? 1200 : 600;
            despairAttack = Mathf.Min(despairAttack + amount, max);
        }

        int passion = Count(SynergyType.Passion);
        if (passion > 0 && enemy != null)
        {
            float duration = passion >= 5 ? 10f : passion >= 3 ? 5f : 2f;
            enemy.ApplyBurn(Mathf.Max(1, stats.CurrentAttack / 10), duration);
        }

        ApplyStats();
    }

    public void OnPlayerDamaged()
    {
        ApplyStats();

        if (Count(SynergyType.Gambler) == 0 || Random.Range(0f, 100f) >= 25f)
            return;

        List<MemoryData> candidates = inventory.GetMemories().FindAll(memory => memory != null && memory.attack <= 0f);
        if (candidates.Count == 0)
            return;

        if (inventory.RemoveMemory(candidates[Random.Range(0, candidates.Count)]))
        {
            gamblerAttack += 25;
            ApplyStats();
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth) => ApplyStats();

    private void Recalculate()
    {
        counts.Clear();
        foreach (MemoryData memory in inventory.GetMemories())
        {
            if (memory == null || memory.synergy == SynergyType.None)
                continue;
            counts[memory.synergy] = Count(memory.synergy) + 1;
        }

        int dreamer = Count(SynergyType.Dreamer);
        inventory.SetAdditionalMemoryCount(dreamer >= 2 ? 10 : dreamer >= 1 ? 5 : 0);
        ApplyStats();
        Changed?.Invoke();
    }

    private void ApplyStats()
    {
        int warrior = Count(SynergyType.StrongWarrior);
        int defense = warrior >= 6 ? 150 : warrior >= 4 ? 100 : warrior >= 2 ? 50 : 0;
        int despair = Count(SynergyType.Despair);
        defense += despair >= 3 ? 0 : despair >= 2 ? -150 : despair >= 1 ? -100 : 0;
        defense += escapeDefense;

        int attack = gamblerAttack + despairAttack;
        if (Count(SynergyType.Fear) >= 1 && stats.currentHealth <= stats.maxHealth * 0.25f)
            attack += Count(SynergyType.Fear) >= 5 ? 100 : Count(SynergyType.Fear) >= 3 ? 50 : 25;

        int petal = Count(SynergyType.Petal);
        int flowerBonus = petal >= 10 ? 33 : petal >= 5 ? 5 : petal >= 1 ? 1 : 0;
        if (stats.currentHealth < stats.maxHealth * 0.75f)
            flowerBonus = 0;

        stats.SetSynergyStats(attack + flowerBonus, defense + flowerBonus,
            roseCharm + flowerBonus, flowerBonus, flowerBonus, flowerBonus);
    }

    private int Count(SynergyType type) => counts.TryGetValue(type, out int count) ? count : 0;
}

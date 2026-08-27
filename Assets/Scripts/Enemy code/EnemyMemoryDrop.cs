using System.Collections.Generic;
using UnityEngine;

public class EnemyMemoryDrop : MonoBehaviour
{
    private const float NormalDropChance = 15f;
    private const float EliteDropChance = 10f;

    [SerializeField] private MemoryDropSettings settings;
    [SerializeField] private Vector2 dropOffset = new Vector2(0f, 0.5f);

    private EnemyStats enemyStats;
    private PlayerStats playerStats;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    public void Initialize(PlayerStats stats)
    {
        playerStats = stats;
    }

    public void TryDrop()
    {
        if (settings == null || settings.PickupPrefab == null || enemyStats == null)
            return;

        bool isElite = enemyStats.Data != null && enemyStats.Data.isElite;
        float baseChance = isElite ? EliteDropChance : NormalDropChance;
        float charmBonus = playerStats != null ? Mathf.Floor(playerStats.CurrentCharm / 10f) : 0f;

        if (Random.Range(0f, 100f) >= Mathf.Min(100f, baseChance + charmBonus))
            return;

        MemoryData memory = SelectMemory(isElite);
        if (memory == null)
            return;

        MemoryPickup pickup = Instantiate(settings.PickupPrefab,
            (Vector2)transform.position + dropOffset, Quaternion.identity);
        pickup.Initialize(memory);
    }

    private MemoryData SelectMemory(bool isElite)
    {
        List<MemoryData> candidates = new List<MemoryData>();

        foreach (MemoryData memory in settings.Memories)
        {
            if (memory == null || memory.type != MemoryType.Memory)
                continue;

            bool allowed = isElite
                ? memory.rarity >= MemoryRarity.Rare
                : memory.rarity <= MemoryRarity.Important;

            if (allowed)
                candidates.Add(memory);
        }

        return candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : null;
    }
}

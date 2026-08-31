using System.Collections.Generic;
using UnityEngine;

// 기억의 조각은 모든 적이, 트라우마의 조각은 정예(트라우마)만 떨어뜨린다.
// 매력은 기억의 조각 확률만 올린다. 트라우마의 조각은 고정 5%다.
public class EnemyMemoryDrop : MonoBehaviour
{
    private const float NormalDropChance = 15f;
    private const float EliteDropChance = 10f;
    private const float TraumaDropChance = 5f;
    private const float GroundSearchHeight = 5f;
    private const float GroundSearchDistance = 50f;

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

        if (isElite && Random.Range(0f, 100f) < TraumaDropChance)
        {
            MemoryData trauma = SelectMemory(MemoryType.Trauma, isElite);
            if (trauma != null)
            {
                Spawn(trauma);
                return;
            }
        }

        float baseChance = isElite ? EliteDropChance : NormalDropChance;
        float charmBonus = playerStats != null ? Mathf.Floor(playerStats.CurrentCharm / 10f) : 0f;

        if (Random.Range(0f, 100f) >= Mathf.Min(100f, baseChance + charmBonus))
            return;

        MemoryData memory = SelectMemory(MemoryType.Memory, isElite);
        if (memory == null)
            return;

        Spawn(memory);
    }

    private void Spawn(MemoryData memory)
    {
        Vector2 spawnPosition = (Vector2)transform.position + dropOffset;
        MemoryPickup pickup = Instantiate(settings.PickupPrefab,
            spawnPosition, Quaternion.identity, transform.parent);
        pickup.Initialize(memory);

        if (TryGetGroundPosition(spawnPosition, out float groundY))
        {
            SpriteRenderer renderer = pickup.GetComponent<SpriteRenderer>();
            float spriteHeight = renderer != null ? renderer.bounds.extents.y : 0f;
            pickup.transform.position = new Vector2(
                spawnPosition.x,
                groundY + spriteHeight);
        }
    }

    private bool TryGetGroundPosition(Vector2 origin, out float groundY)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin + Vector2.up * GroundSearchHeight,
            Vector2.down,
            GroundSearchDistance);

        bool found = false;
        groundY = float.MinValue;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].collider.CompareTag("Ground")
                || hits[i].point.y > origin.y
                || hits[i].point.y <= groundY)
                continue;

            groundY = hits[i].point.y;
            found = true;
        }

        return found;
    }

    private MemoryData SelectMemory(MemoryType type, bool isElite)
    {
        List<MemoryData> candidates = new List<MemoryData>();

        foreach (MemoryData memory in settings.Memories)
        {
            if (memory == null || memory.type != type)
                continue;

            // 트라우마의 조각은 등급 제한 없이 모두 후보가 된다.
            bool allowed = type != MemoryType.Memory
                || (isElite
                    ? memory.rarity >= MemoryRarity.Rare
                    : memory.rarity <= MemoryRarity.Important);

            if (allowed)
                candidates.Add(memory);
        }

        return candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : null;
    }
}

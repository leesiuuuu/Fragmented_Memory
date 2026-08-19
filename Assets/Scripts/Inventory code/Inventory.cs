using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxMemoryCount = 8;

    private PlayerStats playerStats;
    private List<MemoryData> memories = new List<MemoryData>();

    public int MaxMemoryCount => maxMemoryCount;
    public event Action Changed;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public bool AddMemory(MemoryData memory)
    {
        if (memory == null || memories.Count >= maxMemoryCount)
            return false;

        memories.Add(memory);

        if(playerStats != null)
        {
            playerStats.ApplyStat(ConvertToStat(memory));
        }

        Changed?.Invoke();

        return true;
    }

    public bool RemoveMemory(MemoryData memory)
    {
        if (memory == null || !memories.Remove(memory))
            return false;

        if (playerStats != null)
            playerStats.RemoveStat(ConvertToStat(memory));

        Changed?.Invoke();

        return true;
    }

    public List<MemoryData> GetMemories()
    {
        return memories;
    }

    public int GetMemoryCount()
    {
        return memories.Count;
    }

    public bool IsFull()
    {
        return memories.Count >= maxMemoryCount;
    }

    private StatData ConvertToStat(MemoryData memory)
    {
        return new StatData
        {
            health = Mathf.RoundToInt(memory.health),
            attack = Mathf.RoundToInt(memory.attack),
            defense = Mathf.RoundToInt(memory.defense),
            criticalChance = memory.criticalChance,
            criticalDamage = memory.criticalDamage,
            lifeSteal = memory.lifeSteal,
            charm = Mathf.RoundToInt(memory.charm)
        };
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxMemoryCount = 8;

    private PlayerStats playerStats;
    private List<MemoryData> memories = new List<MemoryData>();

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    public bool AddMemory(MemoryData memory)
    {
        if (memories.Count >= maxMemoryCount)
            return false;

        memories.Add(memory);

        if(playerStats != null)
        {
            StatData stat = new StatData
            {
                health = (int)memory.health,
                attack = (int)memory.attack,
                defense = (int)memory.defense,

                criticalChance = memory.criticalChance,
                criticalDamage = memory.criticalDamage,
                lifeSteal = memory.lifeSteal,

                charm = (int)memory.charm
            };

            playerStats.ApplyStat(stat);
        }

        return true;
    }

    public void RemoveMemory(MemoryData memory)
    {
        memories.Remove(memory);
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
}
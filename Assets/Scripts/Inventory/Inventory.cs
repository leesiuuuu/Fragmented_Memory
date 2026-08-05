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
            playerStats.ApplyStat(memory);
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
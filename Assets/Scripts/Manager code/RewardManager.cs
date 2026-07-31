using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("Memory Reward Pool")]
    [SerializeField] private List<MemoryData> memoryPool;

    private List<MemoryData> currentRewards = new List<MemoryData>();


    public void GenerateRewards()
    {
        currentRewards.Clear();

        List<MemoryData> tempPool = new List<MemoryData>(memoryPool);


        for (int i = 0; i < 3; i++)
        {
            if (tempPool.Count == 0)
                break;


            int index = Random.Range(0, tempPool.Count);

            currentRewards.Add(tempPool[index]);

            tempPool.RemoveAt(index);
        }
    }


    public IReadOnlyList<MemoryData> GetRewards()
    {
        return currentRewards;
    }


    public bool SelectReward(int index, Inventory inventory)
    {
        if (inventory == null)
            return false;


        if (index < 0 || index >= currentRewards.Count)
            return false;


        bool added = inventory.AddMemory(currentRewards[index]);


        if (added)
        {
            currentRewards.Clear();
        }


        return added;
    }
}
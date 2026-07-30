using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
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


    public List<MemoryData> GetRewards()
    {
        return currentRewards;
    }


    public void SelectReward(int index, MemoryInventory inventory)
    {
        if (index < 0 || index >= currentRewards.Count)
            return;

        inventory.AddMemory(currentRewards[index]);

        currentRewards.Clear();
    }
}
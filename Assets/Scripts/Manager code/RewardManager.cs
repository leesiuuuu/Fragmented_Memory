using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("보상 후보 목록")]
    [SerializeField] private List<MemoryData> memoryPool;
    private List<MemoryData> currentRewards = new List<MemoryData>();
    private Inventory inventory;

    public event Action RewardSelected;

    public void Initialize(Inventory targetInventory)
    {
        inventory = targetInventory;
    }


    public bool GenerateRewards()
    {
        currentRewards.Clear();

        List<MemoryData> tempPool = memoryPool.FindAll(memory => memory != null);

        if (tempPool.Count == 0)
        {
            Debug.LogError("보상으로 사용할 기억 조각이 없습니다.");
            return false;
        }


        int count = Mathf.Min(3, tempPool.Count);
        for (int i = 0; i < count; i++)
        {
            if (tempPool.Count == 0)
                break;


            int index = UnityEngine.Random.Range(0, tempPool.Count);

            currentRewards.Add(tempPool[index]);

            tempPool.RemoveAt(index);
        }

        return true;
    }


    public IReadOnlyList<MemoryData> GetRewards()
    {
        return currentRewards;
    }

    public bool SelectReward(int index)
    {
        if (inventory == null)
            return false;


        if (index < 0 || index >= currentRewards.Count)
            return false;


        bool added = inventory.AddMemory(currentRewards[index]);


        if (added)
        {
            currentRewards.Clear();
            RewardSelected?.Invoke();
        }


        return added;
    }
}

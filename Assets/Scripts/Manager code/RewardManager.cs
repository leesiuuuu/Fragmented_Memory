using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("보상 후보 목록")]
    [SerializeField] private List<MemoryData> memoryPool;
    [SerializeField] private RewardUI rewardUI;
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

        // 인벤토리가 가득 차면 어떤 조각도 담기지 않는다.
        // 그대로 패널을 띄우면 선택이 계속 실패해 런이 끝나지 않으므로 아예 건너뛴다.
        if (inventory == null || inventory.IsFull())
        {
            Debug.LogWarning("[RewardManager] 인벤토리가 가득 차 보상 선택을 건너뜁니다.");
            return false;
        }

        if (rewardUI == null)
            rewardUI = FindFirstObjectByType<RewardUI>(FindObjectsInactive.Include);

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

        if (rewardUI == null)
        {
            Debug.LogError("RewardUI를 찾을 수 없습니다.");
            currentRewards.Clear();
            return false;
        }

        rewardUI.Open(this, currentRewards);

        return true;
    }

    public IReadOnlyList<MemoryData> GetRewards()
    {
        return currentRewards;
    }

    // 보상을 포기하고 런을 끝낸다.
    // 이 경로가 없으면 선택이 실패할 때 패널이 닫히지 않아 입력이 잠긴 채로 멈춘다.
    public void SkipReward()
    {
        currentRewards.Clear();
        RewardSelected?.Invoke();
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

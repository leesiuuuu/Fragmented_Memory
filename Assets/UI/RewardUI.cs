using System.Collections.Generic;
using UnityEngine;

public class RewardUI : MonoBehaviour
{
    private const string InputLockId = "reward";
    [SerializeField, Min(0f)] private float slotSpacing = 310f;

    private readonly List<RewardSlotUI> slots = new List<RewardSlotUI>();
    private RewardSlotUI slotTemplate;
    private RewardManager rewardManager;
    private bool isInitialized;

    public void Open(RewardManager manager, IReadOnlyList<MemoryData> rewards)
    {
        gameObject.SetActive(true);
        GameplayInputLock.SetLocked(InputLockId, true);
        InitializeSlots();

        rewardManager = manager;
        EnsureSlotCount(rewards.Count);

        float startX = -(rewards.Count - 1) * slotSpacing * 0.5f;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= rewards.Count)
            {
                slots[i].Hide();
                continue;
            }

            RectTransform rect = slots[i].transform as RectTransform;
            if (rect != null)
                rect.anchoredPosition = new Vector2(startX + i * slotSpacing, rect.anchoredPosition.y);

            slots[i].Setup(this, i, rewards[i]);
        }
    }

    public void SelectReward(int index)
    {
        if (rewardManager != null && rewardManager.SelectReward(index))
            Close();
    }

    public void Close()
    {
        GameplayInputLock.SetLocked(InputLockId, false);
        rewardManager = null;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameplayInputLock.SetLocked(InputLockId, false);
    }

    private void InitializeSlots()
    {
        if (isInitialized)
            return;

        slotTemplate = GetComponentInChildren<RewardSlotUI>(true);

        if (slotTemplate == null)
        {
            Debug.LogError("RewardPanel에 RewardSlotUI 템플릿이 없습니다.");
            return;
        }

        slots.Add(slotTemplate);
        isInitialized = true;
    }

    private void EnsureSlotCount(int count)
    {
        if (!isInitialized)
            return;

        while (slots.Count < count)
        {
            RewardSlotUI slot = Instantiate(slotTemplate, slotTemplate.transform.parent);
            slot.name = $"RewardSlot {slots.Count + 1}";
            slots.Add(slot);
        }
    }
}

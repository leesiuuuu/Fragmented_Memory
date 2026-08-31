using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    private const string InputLockId = "reward";
    [SerializeField, Min(0f)] private float slotSpacing = 310f;

    // 비워 둬도 동작한다. 배선하면 인벤토리가 가득 찬 상황에서 수동으로 빠져나올 수 있다.
    [SerializeField] private Button skipButton;

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
        if (rewardManager == null)
            return;

        if (rewardManager.SelectReward(index))
        {
            Close();
            return;
        }

        Debug.LogWarning("[RewardUI] 조각을 담지 못했습니다. 인벤토리가 가득 찼는지 확인하세요.");
    }


    // 보상을 포기한다. Close가 rewardManager를 비우므로 먼저 들고 있어야 한다.
    public void Skip()
    {
        RewardManager manager = rewardManager;

        Close();

        manager?.SkipReward();
    }

    public void Close()
    {
        GameplayInputLock.SetLocked(InputLockId, false);
        rewardManager = null;
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    private void OnDestroy()
    {
        if (skipButton != null)
            skipButton.onClick.RemoveListener(Skip);
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    private const string InputLockId = "reward";

    // 슬롯 사이 간격. 슬롯의 폭과 위치는 레이아웃 그룹이 정한다.
    [SerializeField, Min(0f)] private float slotGap = 24f;

    // 비워 둬도 동작한다. 배선하면 인벤토리가 가득 찬 상황에서 수동으로 빠져나올 수 있다.
    [SerializeField] private Button skipButton;

    private readonly List<RewardSlotUI> slots = new List<RewardSlotUI>();
    private RewardSlotUI slotTemplate;
    private RectTransform slotRow;
    private RewardManager rewardManager;
    private bool isInitialized;

    public void Open(RewardManager manager, IReadOnlyList<MemoryData> rewards)
    {
        gameObject.SetActive(true);
        GameplayInputLock.SetLocked(InputLockId, true);
        InitializeSlots();

        rewardManager = manager;
        EnsureSlotCount(rewards.Count);

        // 좌표 계산은 하지 않는다 — 슬롯 수가 몇 개든 레이아웃 그룹이 가운데로 모은다.
        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= rewards.Count)
            {
                slots[i].Hide();
                continue;
            }

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

        slotRow = BuildSlotRow(slotTemplate);

        slots.Add(slotTemplate);
        isInitialized = true;
    }

    // 슬롯을 가로로 늘어놓는 전용 컨테이너.
    // 패널 루트에 레이아웃 그룹을 붙이면 제목까지 같이 끌려가므로 한 겹 씌운다.
    private RectTransform BuildSlotRow(RewardSlotUI template)
    {
        RectTransform templateRect = template.transform as RectTransform;

        float height = templateRect != null ? templateRect.rect.height : 0f;
        float y = templateRect != null ? templateRect.anchoredPosition.y : 0f;

        GameObject rowObject = new GameObject("SlotRow", typeof(RectTransform));
        RectTransform row = rowObject.GetComponent<RectTransform>();
        row.SetParent(template.transform.parent, false);

        // 패널 폭을 그대로 따라간다 — 해상도가 바뀌어도 슬롯이 밖으로 나가지 않는다.
        row.anchorMin = new Vector2(0f, 0.5f);
        row.anchorMax = new Vector2(1f, 0.5f);
        row.pivot = new Vector2(0.5f, 0.5f);
        row.anchoredPosition = new Vector2(0f, y);
        row.sizeDelta = new Vector2(0f, height);

        HorizontalLayoutGroup layout = rowObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = slotGap;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        template.transform.SetParent(row, false);

        return row;
    }

    private void EnsureSlotCount(int count)
    {
        if (!isInitialized || slotRow == null)
            return;

        while (slots.Count < count)
        {
            RewardSlotUI slot = Instantiate(slotTemplate, slotRow);
            slot.name = $"RewardSlot {slots.Count + 1}";
            slots.Add(slot);
        }
    }
}

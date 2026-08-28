using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private InventorySlotUI slotTemplate;
    [SerializeField] private Transform slotRoot;

    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();

    private void OnEnable()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (inventory != null)
            inventory.Changed += Refresh;

        BuildSlots();
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Changed -= Refresh;
    }

    private void BuildSlots()
    {
        if (inventory == null || slotTemplate == null || slotRoot == null)
            return;

        if (slots.Count == 0)
            slots.Add(slotTemplate);

        while (slots.Count < inventory.MaxMemoryCount)
            slots.Add(Instantiate(slotTemplate, slotRoot));
    }

    private void Refresh()
    {
        if (inventory == null)
            return;

        IReadOnlyList<MemoryData> memories = inventory.GetMemories();

        if (countText != null)
            countText.text = $"기억 조각  {memories.Count} / {inventory.MaxMemoryCount}";

        for (int i = 0; i < slots.Count; i++)
            slots[i].Show(i < memories.Count ? memories[i] : null);
    }
}

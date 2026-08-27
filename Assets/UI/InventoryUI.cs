using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private InventorySlotUI slotTemplate;
    [SerializeField] private Transform slotRoot;
    [SerializeField] private PlayerSynergyManager synergyManager;
    [SerializeField] private SynergySlotUI[] synergySlots;

    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();

    private void OnEnable()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();
        if (synergyManager == null && inventory != null)
            synergyManager = inventory.GetComponent<PlayerSynergyManager>();

        if (inventory != null)
            inventory.Changed += Refresh;
        if (synergyManager != null)
            synergyManager.Changed += RefreshSynergies;

        BuildSlots();
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Changed -= Refresh;
        if (synergyManager != null)
            synergyManager.Changed -= RefreshSynergies;
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

        RefreshSynergies();
    }

    private void RefreshSynergies()
    {
        if (synergyManager == null || synergySlots == null)
            return;

        foreach (SynergySlotUI slot in synergySlots)
        {
            if (slot != null)
                slot.Refresh(synergyManager);
        }
    }
}

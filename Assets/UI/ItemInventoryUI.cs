using System.Collections.Generic;
using UnityEngine;

// ItemInventory의 소모품 목록을 표시하고 선택한 아이템 사용을 ItemEffectApplier에 맡긴다.
// I로 패널을 열고 닫는다. 패널 안에서는 E로 선택한 아이템을 사용한다.
public class ItemInventoryUI : MonoBehaviour
{
    private const string InputLockId = "ItemInventoryUI";

    [SerializeField] private KeyCode toggleKey = KeyCode.I;
    [SerializeField] private KeyCode useKey = KeyCode.E;
    [SerializeField] private ItemInventoryPanel panelPrefab;
    [SerializeField] private ItemEffectApplier effectApplier;

    private readonly List<ItemInventorySlotUI> slots = new List<ItemInventorySlotUI>();
    private ItemInventory inventory;
    private PlayerHP playerHP;
    private ItemInventoryPanel panel;
    private ItemData selectedItem;

    private void Awake()
    {
        inventory = GetComponent<ItemInventory>();
        playerHP = GetComponent<PlayerHP>();

        if (panelPrefab != null)
            panel = Instantiate(panelPrefab);

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.Changed += Refresh;
        if (playerHP != null)
            playerHP.Died += CloseOnDeath;
    }

    private void Update()
    {
        if (panel == null || playerHP == null || playerHP.IsDead)
            return;

        bool isOpen = panel.PanelRoot.activeSelf;

        if (Input.GetKeyDown(toggleKey) && (isOpen || !GameplayInputLock.IsLocked))
        {
            SetVisible(!isOpen);
            return;
        }

        if (isOpen && Input.GetKeyDown(useKey))
            UseSelectedItem();
    }

    private void OnDisable()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (inventory != null)
            inventory.Changed -= Refresh;
        if (playerHP != null)
            playerHP.Died -= CloseOnDeath;
    }

    private void OnDestroy()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (panel != null)
            Destroy(panel.gameObject);
    }

    private void SetVisible(bool visible)
    {
        if (panel == null)
            return;

        panel.PanelRoot.SetActive(visible);
        GameplayInputLock.SetLocked(InputLockId, visible);

        if (visible)
            Refresh();
    }

    private void Select(ItemData item)
    {
        selectedItem = item;
        Refresh();
    }

    private void CloseOnDeath()
    {
        SetVisible(false);
    }

    private void UseSelectedItem()
    {
        if (selectedItem == null || selectedItem.effectType == ItemEffectType.Revival)
            return;

        if (effectApplier == null || !effectApplier.CanApply(selectedItem))
            return;

        effectApplier.Apply(selectedItem);
        inventory.Consume(selectedItem);
    }

    private void Refresh()
    {
        if (panel == null || inventory == null)
            return;

        IReadOnlyList<ItemInventory.Entry> items = inventory.Items;

        if (selectedItem != null && !inventory.Contains(selectedItem))
            selectedItem = null;

        if (slots.Count == 0 && panel.SlotTemplate != null)
            slots.Add(panel.SlotTemplate);

        while (slots.Count < items.Count)
            slots.Add(Instantiate(panel.SlotTemplate, panel.SlotRoot));

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
                slots[i].Setup(items[i], items[i].item == selectedItem, Select);
            else
                slots[i].Hide();
        }

        if (panel.SelectedItemText != null)
        {
            panel.SelectedItemText.text = selectedItem == null
                ? "사용할 아이템을 선택하세요\n\n<size=70%><color=#FFFFFF>I 닫기</color></size>"
                : $"{selectedItem.itemName}\n{selectedItem.description}\n\n<size=70%><color=#FFFFFF>E 사용. I 닫기</color></size>";
        }
    }
}

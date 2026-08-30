using System.Collections.Generic;
using UnityEngine;

// ItemInventory의 소모품 목록을 표시하고 선택한 아이템 사용을 ItemEffectApplier에 맡긴다.
// 소모품 인벤토리는 항상 표시되며, 선택한 아이템은 E로 사용한다.
public class ItemInventoryUI : MonoBehaviour
{
    [SerializeField] private KeyCode useKey = KeyCode.E;
    [SerializeField] private ItemInventoryPanel panelPrefab;
    [SerializeField] private ItemEffectApplier effectApplier;
    [SerializeField] private Canvas targetCanvas;

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
            panel = targetCanvas != null
                ? Instantiate(panelPrefab, targetCanvas.transform)
                : Instantiate(panelPrefab);

        if (panel != null && targetCanvas != null)
        {
            Canvas panelCanvas = panel.GetComponent<Canvas>();
            if (panelCanvas != null)
            {
                panelCanvas.renderMode = targetCanvas.renderMode;
                panelCanvas.worldCamera = targetCanvas.worldCamera;
                panelCanvas.planeDistance = targetCanvas.planeDistance;
            }
        }

        if (panel != null)
        {
            panel.PanelRoot.SetActive(true);
            Refresh();
        }
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

        if (!GameplayInputLock.IsLocked && Input.GetKeyDown(useKey))
            UseSelectedItem();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Changed -= Refresh;
        if (playerHP != null)
            playerHP.Died -= CloseOnDeath;
    }

    private void OnDestroy()
    {
        if (panel != null)
            Destroy(panel.gameObject);
    }

    private void Select(ItemData item)
    {
        selectedItem = item;
        Refresh();
    }

    private void CloseOnDeath()
    {
        selectedItem = null;
        Refresh();
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
        int visibleCount = Mathf.Min(4, items.Count);

        if (selectedItem != null && !inventory.Contains(selectedItem))
            selectedItem = null;

        if (slots.Count == 0 && panel.SlotTemplate != null)
            slots.Add(panel.SlotTemplate);

        if (panel.SlotTemplate == null || panel.SlotRoot == null)
            return;

        while (slots.Count < 4)
            slots.Add(Instantiate(panel.SlotTemplate, panel.SlotRoot));

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < visibleCount)
                slots[i].Setup(items[i], items[i].item == selectedItem,
                    Select, ShowTooltip, HideTooltip);
            else
                slots[i].Hide();
        }

        HideTooltip();
    }

    private void ShowTooltip(ItemData item)
    {
        if (panel.SelectedItemText == null || item == null)
            return;

        panel.SelectedItemText.text =
            $"{item.itemName}\n{item.description}\n\n<size=70%><color=#FFFFFF>E 사용</color></size>";
        panel.SelectedItemText.gameObject.SetActive(true);
    }

    private void HideTooltip()
    {
        if (panel.SelectedItemText != null)
            panel.SelectedItemText.gameObject.SetActive(false);
    }
}

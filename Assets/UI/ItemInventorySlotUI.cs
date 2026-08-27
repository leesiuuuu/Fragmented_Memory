using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image selection;
    [SerializeField] private Button button;

    private ItemData item;

    public void Setup(ItemInventory.Entry entry, bool selected, Action<ItemData> onSelected)
    {
        item = entry.item;
        gameObject.SetActive(true);

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : null;
            icon.enabled = item != null && item.icon != null;
        }

        if (countText != null)
            countText.text = entry.count.ToString();

        if (selection != null)
            selection.enabled = selected;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected?.Invoke(item));
        }
    }

    public void Hide()
    {
        item = null;
        gameObject.SetActive(false);
    }
}

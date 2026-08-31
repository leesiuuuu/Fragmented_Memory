using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemInventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image selection;
    [SerializeField] private Button button;

    private ItemData item;
    private Action<ItemData> onHovered;
    private Action onHoverEnded;

    public void Setup(ItemInventory.Entry entry, bool selected,
        Action<ItemData> onSelected, Action<ItemData> onHovered, Action onHoverEnded)
    {
        item = entry.item;
        this.onHovered = onHovered;
        this.onHoverEnded = onHoverEnded;
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
        onHovered = null;
        onHoverEnded = null;
        gameObject.SetActive(true);

        if (icon != null)
            icon.enabled = false;
        if (countText != null)
            countText.text = string.Empty;
        if (selection != null)
            selection.enabled = false;
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
            onHovered?.Invoke(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverEnded?.Invoke();
    }
}

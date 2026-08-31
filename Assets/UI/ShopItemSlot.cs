using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    private int itemIndex;
    private Action<int> selected;

    private void Awake()
    {
        buyButton.onClick.AddListener(Select);

        RectTransform buttonRect = buyButton.transform as RectTransform;
        if (buttonRect != null)
        {
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = Vector2.zero;
        }

        TMP_Text buttonText = buyButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveListener(Select);
    }

    public void Setup(int index, ItemData item, Action<int> onSelected)
    {
        itemIndex = index;
        selected = onSelected;

        icon.sprite = item.icon;
        icon.enabled = item.icon != null;
        itemNameText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        priceText.gameObject.SetActive(false);
        buyButton.interactable = true;

        gameObject.SetActive(true);
    }

    public void SetSelected(bool isSelected)
    {
        if (buyButton.image != null)
            buyButton.image.color = isSelected
                ? new Color(1f, 0.88f, 0.55f, 0.3f)
                : Color.clear;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Select()
    {
        selected?.Invoke(itemIndex);
    }
}

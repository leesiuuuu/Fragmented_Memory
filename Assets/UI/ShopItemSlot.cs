using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject soldOutObject;

    private ShopManager shopManager;
    private int itemIndex;

    private void Awake()
    {
        buyButton.onClick.AddListener(Buy);
    }

    public void Setup(ShopManager manager, int index, ItemData item, bool isPurchased)
    {
        shopManager = manager;
        itemIndex = index;

        icon.sprite = item.icon;
        icon.enabled = item.icon != null;
        itemNameText.text = item.itemName;
        descriptionText.text = item.description;
        priceText.text = item.price.ToString();
        buyButton.interactable = !isPurchased;

        if (soldOutObject != null)
            soldOutObject.SetActive(isPurchased);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Buy()
    {
        shopManager?.Buy(itemIndex);
    }
}

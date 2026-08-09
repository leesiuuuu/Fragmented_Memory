using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private TMP_Text starDustText;
    [SerializeField] private Button closeButton;
    [SerializeField] private List<ShopItemSlot> slots = new List<ShopItemSlot>();

    private CurrencyWallet wallet;

    public void Initialize(ShopManager manager)
    {
        shopManager = manager;
    }

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        if (shopManager != null)
            shopManager.ShopChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (shopManager != null)
            shopManager.ShopChanged -= Refresh;
        if (wallet != null)
            wallet.StarDustChanged -= UpdateStarDust;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (shopManager == null)
            return;

        BindWallet();

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < shopManager.CurrentItems.Count)
            {
                slots[i].Setup(
                    shopManager,
                    i,
                    shopManager.CurrentItems[i],
                    shopManager.IsPurchased(i));
            }
            else
            {
                slots[i].Hide();
            }
        }
    }

    private void BindWallet()
    {
        if (wallet == shopManager.Wallet)
            return;

        if (wallet != null)
            wallet.StarDustChanged -= UpdateStarDust;

        wallet = shopManager.Wallet;

        if (wallet != null)
        {
            wallet.StarDustChanged += UpdateStarDust;
            UpdateStarDust(wallet.StarDust);
        }
    }

    private void UpdateStarDust(int amount)
    {
        if (starDustText != null)
            starDustText.text = amount.ToString();
    }
}

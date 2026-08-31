using System;
using System.Collections.Generic;
using UnityEngine;

public enum PurchaseResult
{
    Success,
    InvalidItem,
    InsufficientFunds,
    EffectUnavailable
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ItemData> itemPool = new List<ItemData>();
    [SerializeField] private ItemEffectApplier effectApplier;

    private readonly List<ItemData> currentItems = new List<ItemData>();
    private CurrencyWallet wallet;
    private ItemInventory itemInventory;

    public IReadOnlyList<ItemData> CurrentItems => currentItems;
    public CurrencyWallet Wallet => wallet;
    public event Action ShopChanged;
    public event Action<int, ItemData> ItemPurchased;
    public event Action<PurchaseResult> PurchaseFailed;

    public void Initialize(GameObject player)
    {
        wallet = player != null ? player.GetComponent<CurrencyWallet>() : null;
        itemInventory = player != null ? player.GetComponent<ItemInventory>() : null;
        effectApplier?.Initialize(player);
    }

    public void PrepareShop()
    {
        currentItems.Clear();

        foreach (ItemData item in itemPool)
        {
            if (item == null)
                continue;

            currentItems.Add(item);
        }

        ShopChanged?.Invoke();
    }

    public PurchaseResult Buy(int index)
    {
        if (wallet == null || effectApplier == null || index < 0 || index >= currentItems.Count)
            return Fail(PurchaseResult.InvalidItem);

        ItemData item = currentItems[index];

        bool isConsumable = item.itemType == ItemType.Potion;

        if (isConsumable && itemInventory == null)
            return Fail(PurchaseResult.EffectUnavailable);

        if (!isConsumable && !effectApplier.CanApply(item))
            return Fail(PurchaseResult.EffectUnavailable);

        if (!wallet.TrySpend(item.price))
            return Fail(PurchaseResult.InsufficientFunds);

        if (isConsumable)
            itemInventory.Add(item);
        else
            effectApplier.Apply(item);

        ItemPurchased?.Invoke(index, item);
        ShopChanged?.Invoke();
        return PurchaseResult.Success;
    }

    private PurchaseResult Fail(PurchaseResult result)
    {
        PurchaseFailed?.Invoke(result);
        return result;
    }
}

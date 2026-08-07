using System;
using System.Collections.Generic;
using UnityEngine;

public enum PurchaseResult
{
    Success,
    InvalidItem,
    AlreadyPurchased,
    InsufficientFunds,
    EffectUnavailable
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ItemData> itemPool = new List<ItemData>();
    [SerializeField] private ItemEffectApplier effectApplier;

    private readonly List<ItemData> currentItems = new List<ItemData>();
    private readonly List<bool> purchased = new List<bool>();
    private CurrencyWallet wallet;

    public IReadOnlyList<ItemData> CurrentItems => currentItems;
    public CurrencyWallet Wallet => wallet;
    public event Action ShopChanged;
    public event Action<int, ItemData> ItemPurchased;
    public event Action<PurchaseResult> PurchaseFailed;

    public void Initialize(GameObject player)
    {
        wallet = player != null ? player.GetComponent<CurrencyWallet>() : null;
        effectApplier?.Initialize(player);
    }

    public void PrepareShop()
    {
        currentItems.Clear();
        purchased.Clear();

        foreach (ItemData item in itemPool)
        {
            if (item == null)
                continue;

            currentItems.Add(item);
            purchased.Add(false);
        }

        ShopChanged?.Invoke();
    }

    public bool IsPurchased(int index)
    {
        return index >= 0 && index < purchased.Count && purchased[index];
    }

    public PurchaseResult Buy(int index)
    {
        if (wallet == null || effectApplier == null || index < 0 || index >= currentItems.Count)
            return Fail(PurchaseResult.InvalidItem);

        if (purchased[index])
            return Fail(PurchaseResult.AlreadyPurchased);

        ItemData item = currentItems[index];

        if (!effectApplier.CanApply(item))
            return Fail(PurchaseResult.EffectUnavailable);

        if (!wallet.TrySpend(item.price))
            return Fail(PurchaseResult.InsufficientFunds);

        effectApplier.Apply(item);
        purchased[index] = true;
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

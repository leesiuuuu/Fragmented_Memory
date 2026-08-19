using System;
using UnityEngine;

public class CurrencyWallet : MonoBehaviour
{
    [SerializeField, Min(0)] private int starDust;

    public int StarDust => starDust;
    public event Action<int> StarDustChanged;

    public void Add(int amount)
    {
        if (amount <= 0)
            return;

        starDust += amount;
        StarDustChanged?.Invoke(starDust);
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && starDust >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount))
            return false;

        starDust -= amount;
        StarDustChanged?.Invoke(starDust);
        return true;
    }
}

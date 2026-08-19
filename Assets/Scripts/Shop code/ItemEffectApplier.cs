using UnityEngine;

public class ItemEffectApplier : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerHP playerHP;

    public void Initialize(GameObject player)
    {
        if (player == null)
            return;

        playerStats = player.GetComponent<PlayerStats>();
        playerHP = player.GetComponent<PlayerHP>();
    }

    public bool CanApply(ItemData item)
    {
        if (item == null)
            return false;

        if (item.effectType == ItemEffectType.Heal)
            return playerHP != null;

        return playerStats != null;
    }

    public void Apply(ItemData item)
    {
        if (!CanApply(item))
            return;

        if (item.effectType == ItemEffectType.Heal)
        {
            playerHP.Heal(Mathf.RoundToInt(item.effectValue));
            return;
        }

        StatData stat = new StatData();

        switch (item.effectType)
        {
            case ItemEffectType.Attack:
                stat.attack = Mathf.RoundToInt(item.effectValue);
                break;
            case ItemEffectType.Defense:
                stat.defense = Mathf.RoundToInt(item.effectValue);
                break;
            case ItemEffectType.CriticalChance:
                stat.criticalChance = item.effectValue;
                break;
        }

        playerStats.ApplyStat(stat);
    }
}

using UnityEngine;

public enum ItemType
{
    Potion,
    Passive
}

public enum ItemEffectType
{
    Heal,
    Attack,
    Defense,
    CriticalChance,
    AttackCooldown,
    JumpCount,
    SkillCooldown,
    Revival,
    DashCount
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;

    [TextArea]
    public string description;

    public Sprite icon;

    public Sprite detailIcon;

    public int price;

    public ItemType itemType;

    [Header("Effect")]
    public ItemEffectType effectType;

    [Min(0f)]
    public float effectValue;

    [Min(0f)]
    public float duration;

    private void OnValidate()
    {
        price = Mathf.Max(0, price);
        effectValue = Mathf.Max(0f, effectValue);
        duration = Mathf.Max(0f, duration);
    }
}

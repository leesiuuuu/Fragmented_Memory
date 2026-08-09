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
    CriticalChance
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;

    [TextArea]
    public string description;

    public Sprite icon;

    public int price;

    public ItemType itemType;

    [Header("Effect")]
    public ItemEffectType effectType;

    [Min(0f)]
    public float effectValue;

    private void OnValidate()
    {
        price = Mathf.Max(0, price);
        effectValue = Mathf.Max(0f, effectValue);
    }
}

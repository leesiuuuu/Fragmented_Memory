using UnityEngine;

public enum ItemType
{
    Potion,
    Passive
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
}
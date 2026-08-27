using UnityEngine;

public enum MemoryType
{
    Memory,
    Trauma
}

public enum MemoryRarity
{
    Common,
    Rare,
    Important,
    Legendary
}


public enum SynergyType
{
    None, StrongWarrior, OvercomeTrauma, Dreamer, Passion,
    Gambler, Rose, Despair, Escape, Fear, Petal
}

[CreateAssetMenu(fileName = "New Memory", menuName = "Memory/Memory Data")]
public class MemoryData : ScriptableObject
{
    [Header("기본 정보")]
    public string memoryName;

    [TextArea]
    public string description;

    public Sprite icon;


    [Header("분류")]
    public MemoryType type;

    public MemoryRarity rarity;


    public SynergyType synergy;

    [Header("능력치 증가")]
    public float health;
    public float attack;
    public float defense;

    public float criticalChance;
    public float criticalDamage;

    public float lifeSteal;
    public float charm;
}

using UnityEngine;

[CreateAssetMenu]
public class StatData : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    public int HP;
    public int AD;
    public int Defence;
    public int Critical;
    public int CriticalPercentage;
    public int LifeSteal;
    public string Description;

    public SpecialAbilityType SpecialAbility;
}

public enum SpecialAbilityType
{
    None,
    PoisonOnHit,
    Random,
    RandomPlus,
    Turret
}
using UnityEngine;

[System.Serializable]
public class StatData
{
    [Header("기본 스탯")]
    public int health;

    public int attack;

    public int defense;


    [Header("전투 스탯")]
    [Range(0f, 100f)]
    public float criticalChance;

    public float criticalDamage;

    [Range(0f, 100f)]
    public float lifeSteal;


    [Header("특수 스탯")]
    public int charm;
}
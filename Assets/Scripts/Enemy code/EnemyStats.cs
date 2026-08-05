using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHP;
    public int attack;
    public int defense;

    public int currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }
}
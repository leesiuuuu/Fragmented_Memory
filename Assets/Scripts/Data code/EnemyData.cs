using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;

    public GameObject prefab;


    [Header("기본 능력치")]
    // 배율 1.0 기준값. 실제 값은 EnemyStats.Setup(data, power)에서 곱해진다.
    public int maxHP = 100;
    public int attack = 20;
    public int defense = 0;


    [Header("정예")]
    // 정예 방에서만 등장
    public bool isElite;

    public float eliteMultiplier = 2.2f;
}

using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private EnemyData fallbackData;

    public EnemyData Data { get; private set; }

    public int maxHP { get; private set; }
    public int attack { get; private set; }
    public int defense { get; private set; }

    public int currentHP { get; set; }

    private void Awake()
    {
        if (Data == null && fallbackData != null)
            Setup(fallbackData, 1f);
    }

    public void Setup(EnemyData data, float power)
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyStats] {name}: EnemyData가 null입니다.");
            return;
        }

        Data = data;

        float multiplier = power * (data.isElite ? data.eliteMultiplier : 1f);

        maxHP = Mathf.Max(1, Mathf.RoundToInt(data.maxHP * multiplier));
        attack = Mathf.Max(0, Mathf.RoundToInt(data.attack * multiplier));
        defense = Mathf.Max(0, Mathf.RoundToInt(data.defense * multiplier));

        currentHP = maxHP;
    }

    public void SetupBoss(int hp, int bossDefense)
    {
        maxHP = Mathf.Max(1, hp);
        attack = 0;
        defense = Mathf.Max(0, bossDefense);
        currentHP = maxHP;
    }
}
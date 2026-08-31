using UnityEngine;

// BossControl과 EnemyHP를 잇는다.
// 플레이어의 공격은 EnemyHP만 찾으므로 보스도 EnemyHP를 갖고 있어야 피해를 입는다.
// 체력의 주인은 EnemyHP(EnemyStats) 한쪽으로 두고, 같은 피해량을 BossControl에 넘겨
// 체력바와 사망 연출·상태 전환이 따라오게 한다.
[RequireComponent(typeof(BossControl), typeof(EnemyStats), typeof(EnemyHP))]
public class BossHealthBridge : MonoBehaviour
{
    [SerializeField] private int defense;

    private BossControl bossControl;
    private EnemyStats stats;
    private EnemyHP enemyHP;

    private void Awake()
    {
        bossControl = GetComponent<BossControl>();
        stats = GetComponent<EnemyStats>();
        enemyHP = GetComponent<EnemyHP>();

        // 체력 수치는 BossControl에 이미 들어 있다. 그 값을 EnemyStats로 옮겨 한 곳에서만 센다.
        stats.SetupBoss(Mathf.RoundToInt(bossControl.maxHealth), defense);
    }

    private void OnEnable()
    {
        if (enemyHP != null)
            enemyHP.Damaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (enemyHP != null)
            enemyHP.Damaged -= HandleDamaged;
    }

    private void HandleDamaged(int amount)
    {
        if (bossControl != null)
            bossControl.TakeDamage(amount);
    }
}

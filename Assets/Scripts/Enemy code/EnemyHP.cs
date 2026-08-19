using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    private EnemyStats stats;

    private SpawnManager spawnManager;

    [SerializeField] private HPBar hpBar;
    private bool isDead;


    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }


    private void Start()
    {
        hpBar.SetHP(stats.currentHP, stats.maxHP);
    }


    public void SetSpawnManager(SpawnManager manager)
    {
        spawnManager = manager;
    }


    public int TakeDamage(int rawDamage)
    {
        if (isDead)
            return 0;

        int finalDamage = DamageCalculator.Calculate(
            rawDamage,
            stats.defense);
        int effectiveDamage = Mathf.Min(finalDamage, stats.currentHP);

        stats.currentHP -= effectiveDamage;

        // if (effectiveDamage > 0 && EffectManager.Instance != null)
        // {
        //     EffectManager.Instance.Play(
        //         EffectId.EnemyHit,
        //         transform.position,
        //         Quaternion.identity
        //     );
        // }


        hpBar.SetHP(stats.currentHP, stats.maxHP);


        if (stats.currentHP <= 0)
        {
            Die();
        }

        return effectiveDamage;
    }


    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if(spawnManager != null)
        {
            spawnManager.EnemyDead();
        }

        Destroy(gameObject);
    }
}

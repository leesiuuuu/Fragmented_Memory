using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    private EnemyStats stats;

    private SpawnManager spawnManager;

    [SerializeField] private HPBar hpBar;


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


    public void TakeDamage(int damage)
    {
        damage -= stats.defense;

        if (damage < 1)
            damage = 1;


        stats.currentHP -= damage;

        stats.currentHP = Mathf.Max(stats.currentHP, 0);


        hpBar.SetHP(stats.currentHP, stats.maxHP);


        if (stats.currentHP <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        if(spawnManager != null)
        {
            spawnManager.EnemyDead();
        }

        Destroy(gameObject);
    }
}
using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    [SerializeField] private int maxHP = 1300;

    private int currentHP;

    private SpawnManager spawnManager;

    [SerializeField] private HPBar hpBar;


    private void Start()
    {
        currentHP = maxHP;

        hpBar.SetHP(currentHP, maxHP);
    }


    public void SetSpawnManager(SpawnManager manager)
    {
        spawnManager = manager;
    }


    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        currentHP = Mathf.Max(currentHP, 0);


        hpBar.SetHP(currentHP, maxHP);


        if(currentHP <= 0)
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
using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    private int currentHP;

    [SerializeField] private HPBar hpBar;

    private void Start()
    {
        currentHP = maxHP;
        hpBar.SetHP(currentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        hpBar.SetHP(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
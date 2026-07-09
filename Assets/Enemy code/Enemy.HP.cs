using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    public int hp = 100;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
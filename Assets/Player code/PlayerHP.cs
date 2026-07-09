using UnityEngine;

public class PlayerHP : MonoBehaviour
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
        Debug.Log("dead");
        GetComponent<PlayerMovement>().enabled = false;
    }
}

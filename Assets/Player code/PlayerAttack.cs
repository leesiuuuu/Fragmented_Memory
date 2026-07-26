using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int damage = 300;
    public float attackRange = 2f;
    public float attackCoolTime = 0.5f;

    bool canAttack = true;

    void Update()
    {
        AttackInput();
    }

    void AttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canAttack)
        {
            Attack();

            canAttack = false;
            Invoke("ResetAttack", attackCoolTime);
        }
    }

    void Attack()
    {
        FindEnemy();
    }

    void FindEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position,
                                              enemy.transform.position);

            if (distance <= attackRange)
            {
                enemy.GetComponent<EnemyHP>().TakeDamage(damage);
            }
        }
    }

    void ResetAttack()
    {
        canAttack = true;
    }
}
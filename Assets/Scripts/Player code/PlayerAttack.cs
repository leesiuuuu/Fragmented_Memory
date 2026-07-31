using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    Animator animator;

    bool isAttack = false;
    bool canAttack = true;

    public float attackCoolTime = 0.5f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        AttackInput();
    }

    void AttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isAttack && canAttack)
        {
            NormalAttack();
        }
    }

    void NormalAttack()
    {
        isAttack = true;
        canAttack = false;

        animator.SetTrigger("Stroke");

        Invoke("ResetAttackCoolTime", attackCoolTime);
    }

    public void EndAttack()
    {
        isAttack = false;
    }

    void ResetAttackCoolTime()
    {
        canAttack = true;
    }
}
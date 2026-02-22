using UnityEditor.Rendering;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{


    [Header("Attack")]
    [SerializeField] private BoxCollider2D attackCollider;

    private Animator playerAnimator;
    private PlayerController controller;
    private int attackCount = 0;


    private float attackTime;

    private void Start()
    {
        playerAnimator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        checkAttackTime();
    }

    public void Move(bool value)
    {
        playerAnimator.SetBool("Moving", value);
    }

    public void Attack()
    {
        controller.Attacking = true;
        playerAnimator.SetTrigger("Attack");
        playerAnimator.SetInteger("AttackCount", attackCount++);

        if (attackCount > 1)
        {
            attackCount = 0;
        }
    }

    public void Jump(bool value)
    {
        playerAnimator.SetBool("Jump", value);
    }

    public void Skill()
    {
        playerAnimator.SetTrigger("Skill");
    }

    public void Dash()
    {
        playerAnimator.SetTrigger("Dash");
    }

    public void EnableDash()
    {
        controller.Dashing = true;
        controller.Dashable = false;
    }
    public void DisableDash()
    {
        controller.Dashing = false;
        controller.StartDashCooltime();
    }
    public void EnableSkill()
    {
        controller.Skilling = true;
        controller.Skillable = false;
    }

    public void DisableSkill()
    {
        controller.Skilling = false;
        controller.StartSkillCooltime();
    }

    public void EnableAttack()
    {
        controller.Attackable = true;
        attackCollider.enabled = false;
        attackTime = 0f;
    }
    public void DisableAttack()
    {
        controller.Attackable = false;
        attackCollider.enabled = true;
        attackTime = 0f;
    }

    private void checkAttackTime()
    {
        // 현재 공격중인 상태를 확인
        if (!controller.Attacking) return;

        if(attackTime >= 0.5f)
        {
            attackCount = 0;
            attackTime = 0f;
            controller.Attacking = false;
        }
        else
        {
            attackTime += Time.deltaTime;
        }
    }

    public void Death()
    {
        GetComponent<SpriteRenderer>().color = Color.black;
        playerAnimator.SetTrigger("Death");
    }
}

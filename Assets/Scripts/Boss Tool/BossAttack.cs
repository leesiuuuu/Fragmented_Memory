using UnityEngine;

public class BossAttack : IState<BossControl>
{
    private float attackTime;
    private float attackDuration;
    private float cooldownTime;

    public void OperateEnter(BossControl sender)
    {
        attackTime = 0f;
        cooldownTime = 0f;
        attackDuration = sender.GetRandomAttackDuration();

        Debug.Log("Boss Attack 시작");
    }

    public void OperateUpdate(BossControl sender)
    {
        if (!sender.IsPlayerInRange())
        {
            sender.ChangeState(BossControl.BossState.Idle);
            return;
        }

        if (cooldownTime > 0f)
        {
            cooldownTime -= Time.deltaTime;

            if (sender.CanMove())
            {
                Vector3 direction =
                    (sender.Player.position - sender.transform.position).normalized;

                sender.transform.position +=
                    direction * sender.moveSpeed * Time.deltaTime;
            }

            return;
        }

        attackTime += Time.deltaTime;

        if (attackTime >= attackDuration)
        {
            attackTime = 0f;
            attackDuration = sender.GetRandomAttackDuration();

            sender.UseAttackPattern();

            cooldownTime = sender.GetRandomPatternCooldown();
        }
    }

    public void OperateExit(BossControl sender)
    {
        Debug.Log("Boss Attack 종료");
    }
}
using UnityEngine;

public class BossChase : IState<BossControl>
{
    public void OperateEnter(BossControl sender)
    {
        Debug.Log("Boss Chase 시작");
    }

    public void OperateUpdate(BossControl sender)
    {
        if (sender.Player == null)
            return;

        if (!sender.IsPlayerInRange())
        {
            sender.ChangeState(BossControl.BossState.Idle);
            return;
        }

        // 제자리형 보스는 이동을 건너뛰고 곧장 패턴으로 넘어간다.
        if (!sender.chasePlayer)
        {
            sender.ChangeState(BossControl.BossState.Attack);
            return;
        }

        if (!sender.CanMove())
            return;

        Vector3 direction =
            (sender.Player.position - sender.transform.position).normalized;

        sender.transform.position +=
            direction * sender.moveSpeed * Time.deltaTime;

        sender.ChangeState(BossControl.BossState.Attack);
    }

    public void OperateExit(BossControl sender)
    {
        Debug.Log("Boss Chase 종료");
    }
}
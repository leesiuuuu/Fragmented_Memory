using UnityEngine;

public class BossIdle : IState<BossControl>
{
    public void OperateEnter(BossControl sender)
    {
        Debug.Log("Boss Idle 시작");
    }

    public void OperateUpdate(BossControl sender)
    {
        if (sender.IsPlayerInRange())
        {
            sender.ChangeState(BossControl.BossState.Chase);
        }
    }

    public void OperateExit(BossControl sender)
    {
        Debug.Log("Boss Idle 종료");
    }
}
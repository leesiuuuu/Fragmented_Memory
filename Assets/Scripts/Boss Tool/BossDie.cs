using UnityEngine;

public class BossDie : IState<BossControl>
{
    public void OperateEnter(BossControl sender)
    {
        Debug.Log("Boss Die 시작");
    }

    public void OperateUpdate(BossControl sender)
    {
    }

    public void OperateExit(BossControl sender)
    {
        Debug.Log("Boss Die 종료");
    }
}
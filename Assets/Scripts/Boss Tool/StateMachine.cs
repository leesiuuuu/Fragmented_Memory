using UnityEngine;

public class StateMachine<T>
{
    private T m_sender;

    public IState<T> CurState { get; private set; }

    public StateMachine(T sender, IState<T> state)
    {
        m_sender = sender;
        SetState(state);
    }

    public void SetState(IState<T> state)
    {
        if (m_sender == null)
        {
            Debug.LogError("m_sender ERROR");
            return;
        }

        if (CurState == state)
            return;

        if (CurState != null)
            CurState.OperateExit(m_sender);

        CurState = state;

        if (CurState != null)
            CurState.OperateEnter(m_sender);
    }

    public void DoOperateUpdate()
    {
        if (m_sender == null)
        {
            Debug.LogError("invalid m_sender");
            return;
        }

        if (CurState != null)
            CurState.OperateUpdate(m_sender);
    }
}
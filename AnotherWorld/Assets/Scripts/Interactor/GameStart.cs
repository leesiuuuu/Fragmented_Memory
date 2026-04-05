using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : Interactor
{
    [SerializeField] private MirrorWorldRandomizer m_Randomizer;
    public void OnEnable()
    {
        interactionKey = KeyCode.X;
        isActivated = false;
    }
    protected override void interactionEvent()
    {
        m_Randomizer.GameStart();
        base.interactionEvent();
    }
}

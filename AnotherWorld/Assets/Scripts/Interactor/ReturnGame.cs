using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnGame : Interactor
{
    [SerializeField] private MirrorWorldRandomizer m_Randomizer;
    private bool returnable = false;
    private void OnEnable()
    {
        interactionKey = KeyCode.X;
        isActivated = false;
    }

    protected override void Update()
    {
        if (GameManager.Instance.killedEnemy_InGame == GameManager.Instance.SpawnEnemy)
        {
            returnable = true;
        }
        if (returnable)
        {
            base.Update();
        }
    }

    protected override void interactionEvent()
    {
        m_Randomizer.ReturnGame();
        base.interactionEvent();
    }
}

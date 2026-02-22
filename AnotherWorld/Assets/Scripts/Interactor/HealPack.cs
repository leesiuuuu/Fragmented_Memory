using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPack : Interactor
{
    private void OnEnable()
    {
        interactionKey = KeyCode.X;
        isActivated = false;
    }
    protected override void interactionEvent()
    {
        PlayerStatManager.instance.HP = PlayerStatManager.instance.MaxHP;
        base.interactionEvent();
    }
}

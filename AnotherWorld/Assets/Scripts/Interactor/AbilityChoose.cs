using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityChoose : Interactor
{
    [SerializeField] private GameObject canvas;
    private void OnEnable()
    {
        interactionKey = KeyCode.X;
        isActivated = false;
    }

    protected override void interactionEvent()
    {
        // 능력 골라주는 캔퍼스 띄우기
        // ------ //
        canvas.SetActive(true);
        base.interactionEvent();
    }
}

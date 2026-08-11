using System;
using UnityEngine;

public class RoomDoorTrigger : MonoBehaviour, InteractRule
{
    public event Action<RoomDoorTrigger> Entered;

    private bool canUse;
    private bool used;

    private void Awake()
    {
        canUse = false;
        used = false;
    }

    public void SetInteractable(bool value)
    {
        canUse = value;
        used = false;

        gameObject.SetActive(value);
    }

    public void Interact()
    {
        if (!canUse)
            return;

        if (used)
            return;

        used = true;
        Entered?.Invoke(this);
    }
}

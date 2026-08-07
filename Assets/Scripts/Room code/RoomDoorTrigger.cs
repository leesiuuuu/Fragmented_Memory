using System;
using UnityEngine;

public class RoomDoorTrigger : MonoBehaviour
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canUse)
            return;

        if (used)
            return;

        if (!other.CompareTag("Player"))
            return;

        used = true;
        Entered?.Invoke(this);
    }
}

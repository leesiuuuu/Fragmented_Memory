using UnityEngine;

public class RoomDoorTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        isTriggered = true;

        gameManager.MoveNextRoom();
    }
}
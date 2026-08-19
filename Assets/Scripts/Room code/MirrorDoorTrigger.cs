using UnityEngine;

public class MirrorDoorTrigger : MonoBehaviour, InteractRule
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject mirrorDoor;


    private bool used;

    public void Interact()
    {
        if (used)
            return;

        used = true;
        gameManager?.EnterMirror();
        mirrorDoor?.SetActive(false);
    }
}

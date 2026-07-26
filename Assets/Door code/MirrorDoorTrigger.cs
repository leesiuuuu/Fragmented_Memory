using UnityEngine;

public class MirrorDoorTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject mirrorDoor;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            mirrorDoor.SetActive(false);

            gameManager.EnterMirror();
        }
    }
}
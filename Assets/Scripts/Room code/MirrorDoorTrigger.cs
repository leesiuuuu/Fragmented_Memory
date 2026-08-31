using UnityEngine;

public class MirrorDoorTrigger : MonoBehaviour, InteractRule
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject mirrorDoor;


    private bool used;

    // 런이 끝나면 GameManager가 다시 열어 준다.
    // 이게 없으면 used가 영영 true라 거울에 한 번밖에 들어갈 수 없다.
    public void SetAvailable(bool available)
    {
        used = !available;

        if (mirrorDoor != null)
            mirrorDoor.SetActive(available);
    }


    public void Interact()
    {
        if (used)
            return;

        used = true;
        gameManager?.EnterMirror();
        mirrorDoor?.SetActive(false);
    }
}

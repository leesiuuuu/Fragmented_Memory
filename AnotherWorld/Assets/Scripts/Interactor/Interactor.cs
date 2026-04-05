using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] protected GameObject inputKeyObject;
    protected KeyCode interactionKey;
    protected bool onlyOneActivate = true;
    protected bool isActivated = false;
    private bool playerEntered;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isActivated) return;
            playerEntered = true;
            inputKeyObject.SetActive(playerEntered);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isActivated) return;
            playerEntered = false;
            inputKeyObject.SetActive(playerEntered);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (onlyOneActivate && isActivated)
            {
                playerEntered = false;
                inputKeyObject.SetActive(false);
                return;
            }

            playerEntered = true;
            inputKeyObject.SetActive(true);
        }
    }

    protected virtual void Update()
    {
        if (playerEntered)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                if (onlyOneActivate && !isActivated) interactionEvent();
            }
        }
    }

    protected virtual void interactionEvent()
    {
        Debug.Log("키를 눌러서 이벤트 발생함");
        isActivated = true;
        inputKeyObject.SetActive(playerEntered);
    }
}

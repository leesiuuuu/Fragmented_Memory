using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private InteractRule currentTarget;

    private void Update()
    {
        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
            currentTarget.Interact();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        InteractRule target = FindInteractRule(other);

        if (target != null)
            currentTarget = target;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        InteractRule target = FindInteractRule(other);

        if (target != null && target == currentTarget)
            currentTarget = null;
    }

    private InteractRule FindInteractRule(Collider2D other)
    {
        MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is InteractRule interactRule)
                return interactRule;
        }

        return null;
    }
}

using System;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private InteractRule currentTarget;
    private PlayerHP playerHP;
    private bool promptVisible;

    public event Action<bool> InteractionAvailabilityChanged;
    public Transform CurrentTargetTransform
    {
        get
        {
            MonoBehaviour targetBehaviour = currentTarget as MonoBehaviour;
            return targetBehaviour != null ? targetBehaviour.transform : null;
        }
    }

    private void Awake()
    {
        playerHP = GetComponent<PlayerHP>();
    }

    private void Update()
    {
        MonoBehaviour targetBehaviour = currentTarget as MonoBehaviour;

        if (currentTarget != null
            && (targetBehaviour == null || !targetBehaviour.isActiveAndEnabled))
        {
            ClearCurrentTarget();
        }

        bool canInteract = currentTarget != null
            && !GameplayInputLock.IsLocked
            && (playerHP == null || !playerHP.IsDead);

        SetPromptVisible(canInteract);

        if (!canInteract)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentTarget.Interact();
            ClearCurrentTarget();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        InteractRule target = FindInteractRule(other);

        if (target != null)
        {
            currentTarget = target;
            SetPromptVisible(!GameplayInputLock.IsLocked);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        InteractRule target = FindInteractRule(other);

        if (target != null && target == currentTarget)
            ClearCurrentTarget();
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

    private void OnDisable()
    {
        ClearCurrentTarget();
    }

    private void ClearCurrentTarget()
    {
        currentTarget = null;
        SetPromptVisible(false);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptVisible == visible)
            return;

        promptVisible = visible;
        InteractionAvailabilityChanged?.Invoke(visible);
    }
}

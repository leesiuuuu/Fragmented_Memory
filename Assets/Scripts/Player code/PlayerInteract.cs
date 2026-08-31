using System;
using UnityEngine;

// 문과 거울과 상점과 기억 조각의 상호작용을 한 곳에서 처리한다.
// 소모품 사용은 V로 따로 받아 상호작용 입력과 겹치지 않게 한다.
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.C;

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

        if (Input.GetKeyDown(interactKey))
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

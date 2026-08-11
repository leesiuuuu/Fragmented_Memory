using TMPro;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private RectTransform promptRect;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string message = "[E] \uC0C1\uD638\uC791\uC6A9";
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    private PlayerInteract playerInteract;
    private Camera targetCamera;

    private void OnEnable()
    {
        playerInteract = FindFirstObjectByType<PlayerInteract>();
        targetCamera = Camera.main;

        if (promptText != null)
            promptText.text = message;

        SetVisible(false);

        if (playerInteract != null)
            playerInteract.InteractionAvailabilityChanged += SetVisible;
    }

    private void LateUpdate()
    {
        if (promptRoot == null || !promptRoot.activeSelf || playerInteract == null)
            return;

        Transform target = playerInteract.CurrentTargetTransform;
        if (target == null)
        {
            SetVisible(false);
            return;
        }

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null && promptRect != null)
            promptRect.position = targetCamera.WorldToScreenPoint(target.position + worldOffset);
    }

    private void OnDisable()
    {
        if (playerInteract != null)
            playerInteract.InteractionAvailabilityChanged -= SetVisible;
    }

    private void SetVisible(bool visible)
    {
        if (promptRoot != null)
            promptRoot.SetActive(visible);
    }
}

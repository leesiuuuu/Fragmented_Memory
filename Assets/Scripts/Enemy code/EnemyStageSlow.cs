using UnityEngine;

public class EnemyStageSlow : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 0.5f;

    private static PlayerMovement playerMovement;
    private static int activeCount;

    private bool isApplied;


    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Apply(player.GetComponent<PlayerMovement>());
    }


    private void Apply(PlayerMovement target)
    {
        if (target == null || isApplied)
            return;

        if (activeCount == 0)
        {
            playerMovement = target;
            target.SetExternalMovementMultiplier(speedMultiplier);
        }

        activeCount++;
        isApplied = true;
    }


    private void Release()
    {
        if (!isApplied)
            return;

        activeCount = Mathf.Max(0, activeCount - 1);
        isApplied = false;

        if (activeCount > 0)
            return;

        if (playerMovement != null)
            playerMovement.SetExternalMovementMultiplier(1f);

        playerMovement = null;
    }


    private void OnDisable()
    {
        Release();
    }
}

using System.Collections;
using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    [SerializeField] private float hitInvincibilityDuration = 0.5f;

    private bool isHitInvincible;
    private bool isDashInvincible;

    public bool IsInvincible => isHitInvincible || isDashInvincible;



    public void StartHitInvincibility()
    {
        if(hitInvincibilityDuration <= 0f)
            return;

        StartCoroutine(HitInvincibility());
    }


    public void StartDashInvincibility()
    {
        isDashInvincible = true;
    }


    public void EndDashInvincibility()
    {
        isDashInvincible = false;
    }



    private IEnumerator HitInvincibility()
    {
        isHitInvincible = true;

        yield return new WaitForSeconds(hitInvincibilityDuration);

        isHitInvincible = false;
    }

    private void OnDisable()
    {
        isHitInvincible = false;
        isDashInvincible = false;
    }
}

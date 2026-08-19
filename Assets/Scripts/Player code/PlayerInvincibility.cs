using System.Collections;
using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    [SerializeField] private float hitInvincibilityDuration = 0.5f;

    private bool isHitInvincible;

    public bool IsInvincible => isHitInvincible;

    // TODO: 대시 시작과 종료 시 별도 무적 상태를 변경하고 IsInvincible 판정에 포함한다.



    public void StartHitInvincibility()
    {
        if(hitInvincibilityDuration <= 0f)
            return;

        StartCoroutine(HitInvincibility());
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
    }
}

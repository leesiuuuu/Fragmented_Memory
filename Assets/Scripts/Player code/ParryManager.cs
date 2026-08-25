using System;
using UnityEngine;

public class ParryManager : MonoBehaviour
{
    [SerializeField] private KeyCode parryKey = KeyCode.X;
    [SerializeField] private float parryDuration = 0.2f;
    [SerializeField] private Vector2 parrySize = new Vector2(2f, 1.5f);
    [SerializeField] private Vector2 parryOffset = new Vector2(1f, 0f);
    [SerializeField] private LayerMask parryLayers = ~0;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHP playerHP;
    private PlayerCombat combat;
    private float parryEndTime;

    public bool CanPoke { get; private set; }
    public event Action ParrySucceeded;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHP = GetComponent<PlayerHP>();
        combat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        if (playerHP.IsDead || GameplayInputLock.IsLocked)
            return;

        if (Input.GetKeyDown(parryKey) && !combat.IsBusy)
        {
            combat.StartAction();
            parryEndTime = Time.time + parryDuration;
            animator.SetTrigger("Parry");
        }
    }

    public bool TryParry()
    {
        if (Time.time > parryEndTime)
            return false;

        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 center = (Vector2)transform.position
            + new Vector2(parryOffset.x * direction.x, parryOffset.y);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, parrySize, 0f, parryLayers);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponentInParent<EnemyAttack>() == null
                && hits[i].GetComponentInParent<EnemyProjectile>() == null)
                continue;

            parryEndTime = 0f;
            CanPoke = true;
            ParrySucceeded?.Invoke();
            return true;
        }

        return false;
    }

    public bool ConsumePoke()
    {
        if (!CanPoke)
            return false;

        CanPoke = false;
        return true;
    }

    public void EndParry()
    {
        combat.EndAction();
    }

    private void OnDrawGizmosSelected()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        float direction = renderer != null && renderer.flipX ? -1f : 1f;
        Vector2 center = (Vector2)transform.position
            + new Vector2(parryOffset.x * direction, parryOffset.y);
        Gizmos.DrawWireCube(center, parrySize);
    }
}

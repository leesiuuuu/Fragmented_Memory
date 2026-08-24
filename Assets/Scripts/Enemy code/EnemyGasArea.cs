using System.Collections.Generic;
using UnityEngine;

public class EnemyGasArea : MonoBehaviour
{
    [SerializeField] private Collider2D hitbox;

    private int damagePerTick;
    private float damageInterval;
    private Vector3 destination;
    private float travelSpeed;
    private bool isTravelling;
    private readonly List<Collider2D> hits = new List<Collider2D>(16);
    private ContactFilter2D damageFilter;


    private void Awake()
    {
        damageFilter = ContactFilter2D.noFilter;
    }


    public void Initialize(
        int damagePerSecond,
        float duration,
        float damageInterval,
        bool flipX,
        float direction,
        float travelDistance,
        float travelDuration)
    {
        damagePerTick = Mathf.RoundToInt(damagePerSecond * damageInterval);
        this.damageInterval = damageInterval;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.flipX = flipX;

        destination = transform.position + Vector3.right * direction * travelDistance;
        travelSpeed = travelDuration > 0f ? travelDistance / travelDuration : 0f;
        isTravelling = travelDuration > 0f && travelDistance > 0f;

        if (isTravelling)
            Invoke(nameof(StartDamage), travelDuration);
        else
            StartDamage();

        Destroy(gameObject, travelDuration + duration);
    }


    private void Update()
    {
        if (!isTravelling)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            travelSpeed * Time.deltaTime);
    }


    private void StartDamage()
    {
        transform.position = destination;
        isTravelling = false;
        InvokeRepeating(nameof(ApplyDamage), 0f, damageInterval);
    }


    private void ApplyDamage()
    {
        if (hitbox == null)
            return;

        Bounds bounds = hitbox.bounds;
        hits.Clear();
        Physics2D.OverlapBox(bounds.center, bounds.size, 0f, damageFilter, hits);

        foreach (Collider2D hit in hits)
        {
            PlayerHP playerHP = hit.GetComponentInParent<PlayerHP>();

            if (playerHP == null)
                continue;

            playerHP.TakeDamage(damagePerTick);
            return;
        }
    }


    private void OnDisable()
    {
        CancelInvoke();
    }
}

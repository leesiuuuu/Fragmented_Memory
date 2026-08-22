using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private Collider2D hitbox;

    private int damage;
    private Collider2D ownerCollider;
    private Transform target;
    private float speed;
    private float homingEndTime;
    private float maxDistance;
    private Vector2 startPosition;


    public void Initialize(
        int attackDamage,
        Vector2 direction,
        float speed,
        float lifetime,
        Collider2D owner,
        Transform followTarget,
        float homingDuration,
        float maxTravelDistance)
    {
        damage = attackDamage;
        ownerCollider = owner;
        target = followTarget;
        this.speed = speed;
        maxDistance = maxTravelDistance;
        startPosition = transform.position;
        homingEndTime = Time.time + homingDuration;

        if (hitbox != null && ownerCollider != null)
            Physics2D.IgnoreCollision(hitbox, ownerCollider);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0f;

        rigid.linearVelocity = direction * speed;
        Destroy(gameObject, lifetime);
    }


    private void FixedUpdate()
    {
        if (maxDistance > 0f
            && Vector2.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || Time.time >= homingEndTime)
            return;

        Vector2 direction = (target.position - transform.position).normalized;
        rigid.linearVelocity = direction * speed;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == ownerCollider)
            return;

        PlayerHP playerHP = other.GetComponentInParent<PlayerHP>();

        if (playerHP != null)
        {
            playerHP.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}

using UnityEngine;

// EnemyAttack에서 받은 피해와 이동값으로 투사체를 움직이고 플레이어에게 피해를 준다.
// 직선 이동과 유도 이동은 속도를 갱신한다. 포물선 이동은 초기 속도 이후 Rigidbody2D 중력을 따른다.
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private Collider2D hitbox;
    [SerializeField] private bool useParabolicTrajectory;
    [SerializeField] private float arcHeight = 2f;

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

        if (!TrySetParabolicVelocity())
            rigid.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }


    private bool TrySetParabolicVelocity()
    {
        if (!useParabolicTrajectory || target == null)
            return false;

        float gravity = Mathf.Abs(Physics2D.gravity.y * rigid.gravityScale);

        if (gravity <= 0f || arcHeight <= 0f)
            return false;

        Vector2 distance = target.position - transform.position;
        float upwardTime = Mathf.Sqrt(2f * arcHeight / gravity);
        float downwardHeight = Mathf.Max(arcHeight - distance.y, 0f);
        float downwardTime = Mathf.Sqrt(2f * downwardHeight / gravity);
        float flightTime = upwardTime + downwardTime;

        if (flightTime <= 0f)
            return false;

        rigid.linearVelocity = new Vector2(
            distance.x / flightTime,
            gravity * upwardTime);
        return true;
    }


    private void FixedUpdate()
    {
        if (maxDistance > 0f
            && Vector2.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        if (useParabolicTrajectory || target == null || Time.time >= homingEndTime)
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

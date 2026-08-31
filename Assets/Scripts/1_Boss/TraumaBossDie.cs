using UnityEngine;

public class TraumaBossDie : MonoBehaviour
{
    public float fallSpeed = 3f;
    public float shrinkSpeed = 1f;
    public float deathScale = 0.2f;
    public float rayDistance = 20f;

    public float deathGroundOffset = 0.2f;

    public LayerMask groundLayer;

    private BossControl boss;
    private BoxCollider2D box;

    private bool isDead;
    private bool isGround;

    private Vector3 startScale;
    private float groundY;

    void Start()
    {
        boss = GetComponent<BossControl>();
        box = GetComponent<BoxCollider2D>();

        startScale = transform.localScale;

        if (boss != null)
            boss.OnDeath += Die;
    }

    void OnDestroy()
    {
        if (boss != null)
            boss.OnDeath -= Die;
    }

    void Update()
    {
        if (!isDead)
            return;

        if (!isGround)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(
                    transform.position.x,
                    groundY,
                    transform.position.z
                ),
                fallSpeed * Time.deltaTime
            );

            if (transform.position.y <= groundY + 0.01f)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    groundY,
                    transform.position.z
                );

                isGround = true;
            }
        }

        Vector3 targetScale = startScale * deathScale;

        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            targetScale,
            shrinkSpeed * Time.deltaTime
        );
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            rayDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            if (box != null)
            {
                groundY =
                    hit.point.y +
                    box.bounds.extents.y +
                    deathGroundOffset;
            }
            else
            {
                groundY =
                    hit.point.y +
                    deathGroundOffset;
            }
        }
        else
        {
            groundY =
                transform.position.y -
                rayDistance;

            Debug.LogWarning(
                gameObject.name +
                " : 아래에서 Ground를 찾지 못했습니다."
            );
        }
    }
}
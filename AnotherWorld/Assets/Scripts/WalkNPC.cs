using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class WalkNPC : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;      // 이동 속도
    [SerializeField] private float minWalkTime = 1f;    // 최소 이동 시간
    [SerializeField] private float maxWalkTime = 3f;    // 최대 이동 시간
    [SerializeField] private float minWaitTime = 1f;    // 최소 대기 시간
    [SerializeField] private float maxWaitTime = 2f;    // 최대 대기 시간

    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private LayerMask wallLayer;

    private Rigidbody2D rb;
    private Vector2 movementDirection;
    private Animator animator;

    void Awake()
    {
        // Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();

        // 2D 게임에서 회전 방지 (필요 시)
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        StartCoroutine(WanderRoutine());
    }

    private bool IsWallAhead()
    {
        Vector2 origin = transform.position;
        Vector2 direction = new Vector2(transform.localScale.x, 0f);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, wallCheckDistance, wallLayer);

        return hit.collider != null;
    }
    private IEnumerator WanderRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        animator.SetInteger("deathState", 1);
        yield return new WaitForSeconds(0.5f);
        animator.SetInteger("deathState", 2);
        yield return new WaitForSeconds(0.5f);
        animator.SetInteger("deathState", 3);
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            movementDirection = new Vector2(Random.Range(-1f, 1f), 0f).normalized;

            transform.localScale = new Vector3(movementDirection.x > 0 ? 1f : -1f, 1f, 1f);

            float walkTime = Random.Range(minWalkTime, maxWalkTime);
            float timer = 0f;

            animator.SetBool("Moving", true);
            while (timer < walkTime)
            {
                if (IsWallAhead())
                {
                    rb.velocity = Vector2.zero;

                    movementDirection.x *= -1;
                    transform.localScale = new Vector3(
                        movementDirection.x > 0 ? 1f : -1f,
                        1f,
                        1f
                    );

                    break;
                }
                rb.velocity = Vector2.Lerp(rb.velocity, movementDirection * moveSpeed, 0.1f);
                timer += Time.deltaTime;
                yield return null;
            }

            // 감속
            while (rb.velocity.magnitude > 0.1f)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
                yield return null;
            }

            animator.SetBool("Moving", false);
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }
}
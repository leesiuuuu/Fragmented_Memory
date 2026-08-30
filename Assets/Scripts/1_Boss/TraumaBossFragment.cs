using System.Collections;
using UnityEngine;

public class TraumaBossFragment : MonoBehaviour
{
    public GameObject fragmentPrefab;

    public int fragmentCount = 2;
    public float downSpeed = 8f;
    public float fragmentSpeed = 8f;
    public float rayDistance = 10f;
    public float stopHeight = 1.5f;
    public float fragmentHeight = 1f;
    public float spreadAngle = 30f;
    public LayerMask groundLayer;

    private BossControl boss;
    private bool isAttacking;

    void Start()
    {
        boss = GetComponent<BossControl>();

        if (boss != null)
            boss.OnPatternSelected += Pattern;
    }

    void OnDestroy()
    {
        if (boss != null)
            boss.OnPatternSelected -= Pattern;
    }

    void Pattern(int patternID, float damage)
    {
        if (patternID == 2 && !isAttacking)
            StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        isAttacking = true;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            rayDistance,
            groundLayer
        );

        if (hit.collider == null)
        {
            isAttacking = false;
            yield break;
        }

        float distance = transform.position.y - hit.point.y;

        if (distance > stopHeight)
        {
            Vector3 target = transform.position;
            target.y = hit.point.y + stopHeight;

            while (Vector2.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    downSpeed * Time.deltaTime
                );

                yield return null;
            }
        }

        boss.PlayAttackMotion(2);

        Vector3 shootPosition = transform.position;
        shootPosition.y += fragmentHeight;

        CreateFragment(Vector2.left, shootPosition);
        CreateFragment(Vector2.right, shootPosition);

        Vector2 leftDirection = Quaternion.Euler(
            0f,
            0f,
            -spreadAngle
        ) * Vector2.left;

        Vector2 rightDirection = Quaternion.Euler(
            0f,
            0f,
            spreadAngle
        ) * Vector2.right;

        CreateFragment(leftDirection, shootPosition);
        CreateFragment(rightDirection, shootPosition);

        isAttacking = false;
    }

    void CreateFragment(Vector2 direction, Vector3 position)
    {
        GameObject fragment = Instantiate(
            fragmentPrefab,
            position,
            Quaternion.identity
        );

        TraumaFragment traumaFragment =
            fragment.GetComponent<TraumaFragment>();

        if (traumaFragment != null)
            traumaFragment.SetBoss(boss);

        Rigidbody2D rigid = fragment.GetComponent<Rigidbody2D>();

        if (rigid != null)
        {
            rigid.gravityScale = 0f;
            rigid.linearVelocity =
                direction.normalized * fragmentSpeed;
        }
    }
}
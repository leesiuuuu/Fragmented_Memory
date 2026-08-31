using System.Collections;
using UnityEngine;

public class BlackEyeSideAttack : MonoBehaviour
{
    public GameObject clonePrefab;

    public Transform leftPoint;
    public Transform rightPoint;

    public Transform centerPoint;
    public Transform centerPointClone;

    public float moveSpeed = 3f;

    public float startScale = 1f;
    public float endScale = 1.2f;

    private BossControl boss;
    private bool isAttacking;

    private Vector3 originalScale;
    private GameObject clone;

    void Awake()
    {
        boss = GetComponent<BossControl>();
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        if (boss == null)
            boss = GetComponent<BossControl>();

        if (boss != null)
            boss.OnPatternSelected += Pattern;
    }

    void OnDisable()
    {
        if (boss != null)
            boss.OnPatternSelected -= Pattern;

        if (clone != null)
        {
            Destroy(clone);
            clone = null;
        }

        transform.localScale = originalScale;
        isAttacking = false;
    }

    void Pattern(int patternID, float damage)
    {
        if (patternID == 1 && !isAttacking)
            StartCoroutine(SideAttack());
    }

    IEnumerator SideAttack()
    {
        isAttacking = true;

        if (boss == null ||
            clonePrefab == null ||
            leftPoint == null ||
            rightPoint == null ||
            centerPoint == null ||
            centerPointClone == null)
        {
            isAttacking = false;
            yield break;
        }

        boss.SetCanMove(false);

        transform.position = leftPoint.position;

        clone = Instantiate(
            clonePrefab,
            rightPoint.position,
            Quaternion.identity
        );

        Vector3 cloneStartScale = clone.transform.localScale;

        float distance = Vector3.Distance(
            leftPoint.position,
            centerPoint.position
        );

        if (distance <= 0.01f)
        {
            transform.position = centerPoint.position;

            if (clone != null)
                clone.transform.position = centerPointClone.position;

            if (clone != null)
                Destroy(clone);

            transform.position = leftPoint.position;

            boss.SetCanMove(true);
            isAttacking = false;
            yield break;
        }

        float time = 0f;

        boss.PlayAttackMotion(1);

        while (time < 1f)
        {
            time += moveSpeed / distance * Time.deltaTime;

            if (time > 1f)
                time = 1f;

            transform.position = Vector3.Lerp(
                leftPoint.position,
                centerPoint.position,
                time
            );

            transform.localScale = Vector3.Lerp(
                originalScale * startScale,
                originalScale * endScale,
                time
            );

            if (clone != null)
            {
                clone.transform.position = Vector3.Lerp(
                    rightPoint.position,
                    centerPointClone.position,
                    time
                );

                clone.transform.localScale = Vector3.Lerp(
                    cloneStartScale * startScale,
                    cloneStartScale * endScale,
                    time
                );
            }

            yield return null;
        }

        transform.position = centerPoint.position;

        if (clone != null)
            clone.transform.position = centerPointClone.position;

        if (clone != null)
        {
            Destroy(clone);
            clone = null;
        }

        transform.localScale = originalScale;

        transform.position = leftPoint.position;

        boss.SetCanMove(true);
        isAttacking = false;
    }
}   
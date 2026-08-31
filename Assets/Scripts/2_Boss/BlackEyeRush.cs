using System.Collections;
using UnityEngine;

public class BlackEyeRush : MonoBehaviour
{
    public GameObject clonePrefab;

    public Transform leftPoint;
    public Transform rightPoint;
    public Transform centerPoint;
    public Transform centerPointClone;
    public Transform topPoint;
    public Transform topPointClone;

    public float moveSpeed = 12f;
    public float fallSpeed = 20f;
    public float waitTime = 0.2f;

    [Header("Motion")]
    public Sprite[] motionSprites;
    public float motionSpeed = 0.08f;
    public SpriteRenderer bossRenderer;

    private BossControl boss;
    private bool isAttacking;
    private GameObject clone;

    void Awake()
    {
        boss = GetComponent<BossControl>();

        if (bossRenderer == null)
            bossRenderer = GetComponent<SpriteRenderer>();
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

        if (bossRenderer != null)
            bossRenderer.enabled = true;

        if (clone != null)
        {
            Destroy(clone);
            clone = null;
        }

        isAttacking = false;
    }

    void Pattern(int patternID, float damage)
    {
        if (patternID == 3 && !isAttacking)
            StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        if (boss == null ||
            boss.Player == null ||
            clonePrefab == null ||
            leftPoint == null ||
            rightPoint == null ||
            centerPoint == null ||
            centerPointClone == null ||
            topPoint == null ||
            topPointClone == null)
        {
            isAttacking = false;
            yield break;
        }

        boss.SetCanMove(false);

        transform.position = leftPoint.position;
        yield return MoveTo(centerPoint.position);

        transform.position = rightPoint.position;
        yield return MoveTo(centerPointClone.position);

        transform.position = leftPoint.position;
        yield return MoveTo(centerPoint.position);

        transform.position = rightPoint.position;
        yield return MoveTo(centerPointClone.position);

        transform.position = topPoint.position;

        clone = Instantiate(
            clonePrefab,
            topPointClone.position,
            Quaternion.identity
        );

        clone.SetActive(false);

        yield return new WaitForSeconds(waitTime);

        Vector3 playerPosition = boss.Player.position;

        bool playerOnLeft =
            playerPosition.x < centerPoint.position.x;

        Transform bossCenter;
        Transform cloneCenter;

        if (playerOnLeft)
        {
            bossCenter = centerPoint;
            cloneCenter = centerPointClone;
        }
        else
        {
            bossCenter = centerPointClone;
            cloneCenter = centerPoint;
        }

        Vector3 bossTarget = new Vector3(
            playerPosition.x,
            bossCenter.position.y,
            transform.position.z
        );

        StartCoroutine(PlayMotion());

        yield return FallTo(bossTarget);

        if (bossRenderer != null)
            bossRenderer.enabled = false;

        yield return new WaitForSeconds(waitTime);

        if (clone != null)
        {
            clone.SetActive(true);

            Vector3 cloneTarget = new Vector3(
                playerPosition.x,
                cloneCenter.position.y,
                clone.transform.position.z
            );

            yield return FallCloneTo(cloneTarget);

            Destroy(clone);
            clone = null;
        }

        if (bossRenderer != null)
            bossRenderer.enabled = true;

        transform.position = rightPoint.position;

        boss.SetCanMove(true);
        isAttacking = false;
    }

    IEnumerator PlayMotion()
    {
        if (motionSprites == null ||
            motionSprites.Length == 0 ||
            bossRenderer == null)
            yield break;

        for (int i = 0; i < motionSprites.Length; i++)
        {
            if (motionSprites[i] != null)
                bossRenderer.sprite = motionSprites[i];

            yield return new WaitForSeconds(motionSpeed);
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        while (Vector2.Distance(
            transform.position,
            target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }

    IEnumerator FallTo(Vector3 target)
    {
        while (Vector2.Distance(
            transform.position,
            target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                fallSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }

    IEnumerator FallCloneTo(Vector3 target)
    {
        while (clone != null &&
               Vector2.Distance(
                   clone.transform.position,
                   target) > 0.05f)
        {
            clone.transform.position = Vector3.MoveTowards(
                clone.transform.position,
                target,
                fallSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (clone != null)
            clone.transform.position = target;
    }
}
using System.Collections;
using UnityEngine;

public class TraumaBossRush : MonoBehaviour
{
    public float backDistance = 2f;
    public float backSpeed = 5f;
    public float rushSpeed = 15f;
    public float diveSpeed = 20f;
    public float riseSpeed = 8f;
    public float attackHeight = 3f;

    private BossControl boss;
    private bool isAttacking;

    void Start()
    {
        boss = GetComponent<BossControl>();

        if (boss != null)
            boss.OnPatternSelected += OnPatternSelected;
    }

    void OnDestroy()
    {
        if (boss != null)
            boss.OnPatternSelected -= OnPatternSelected;
    }

    void OnPatternSelected(int patternID, float attackPower)
    {
        if (patternID == 1)
            StartRush();
    }

    void StartRush()
    {
        if (!isAttacking && boss.Player != null)
            StartCoroutine(Rush());
    }

    IEnumerator Rush()
    {
        isAttacking = true;

        Transform player = boss.Player;

        float direction =
            transform.position.x < player.position.x
                ? -1f
                : 1f;

        Vector3 backPosition = new Vector3(
            transform.position.x +
            direction * backDistance,
            transform.position.y,
            transform.position.z
        );

        while (Vector2.Distance(
            transform.position,
            backPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                backPosition,
                backSpeed * Time.deltaTime
            );

            yield return null;
        }

        Vector3 abovePosition = new Vector3(
            player.position.x,
            player.position.y + attackHeight,
            transform.position.z
        );

        while (Vector2.Distance(
            transform.position,
            abovePosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                abovePosition,
                rushSpeed * Time.deltaTime
            );

            yield return null;
        }

        boss.PlayAttackMotion(1);

        Vector3 attackPosition = new Vector3(
            transform.position.x,
            player.position.y,
            transform.position.z
        );

        while (transform.position.y >
               attackPosition.y + 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                attackPosition,
                diveSpeed * Time.deltaTime
            );

            yield return null;
        }

        Vector3 risePosition = new Vector3(
            transform.position.x,
            abovePosition.y,
            transform.position.z
        );

        while (Vector2.Distance(
            transform.position,
            risePosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                risePosition,
                riseSpeed * Time.deltaTime
            );

            yield return null;
        }

        isAttacking = false;
    }
}
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
    private BossGroundLock groundLock;
    private bool isAttacking;

    void Start()
    {
        boss = GetComponent<BossControl>();
        groundLock = GetComponent<BossGroundLock>();

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

        // 점프 패턴 동안에는 바닥 고정을 풀어야 위로 뜰 수 있다.
        if (groundLock != null)
            groundLock.Suspend();

        Transform player = boss.Player;

        // 패턴이 끝나면 돌아올 지면 높이
        float groundY = transform.position.y;

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

        // 예전에는 다시 공중으로 올라가 그대로 머물렀다.
        // 중력이 없어 스스로 내려오지 못하므로 시작 높이로 돌려놓는다.
        Vector3 returnPosition = new Vector3(
            transform.position.x,
            groundY,
            transform.position.z
        );

        while (Mathf.Abs(transform.position.y - returnPosition.y) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                returnPosition,
                riseSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (groundLock != null)
            groundLock.Resume();

        isAttacking = false;
    }
}
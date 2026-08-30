using System.Collections;
using UnityEngine;

public class BossJumpDashPattern : MonoBehaviour
{
    public float jumpPower = 6f;
    public float jumpTime = 0.25f;
    public float dashSpeed = 24f;

    [Header("Effect")]
    public Sprite[] dashEffects;
    public float effectSpeed = 0.05f;
    public SpriteRenderer effectRenderer;

    private BossControl boss;
    private Rigidbody2D rigid;
    private bool isAttacking;

    void Awake()
    {
        boss = GetComponent<BossControl>();
        rigid = GetComponent<Rigidbody2D>();

        if (effectRenderer != null)
            effectRenderer.enabled = false;
    }

    void OnEnable()
    {
        if (boss == null)
            boss = GetComponent<BossControl>();

        boss.OnPatternSelected += Pattern;
    }

    void OnDisable()
    {
        if (boss != null)
            boss.OnPatternSelected -= Pattern;

        if (effectRenderer != null)
            effectRenderer.enabled = false;
    }

    void Pattern(int patternID, float damage)
    {
        if (patternID == 3 && !isAttacking)
            StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        boss.SetCanMove(false);

        rigid.linearVelocity = new Vector2(
            rigid.linearVelocity.x,
            jumpPower
        );

        yield return new WaitForSeconds(jumpTime);

        if (boss.Player == null)
        {
            boss.SetCanMove(true);
            isAttacking = false;
            yield break;
        }

        Vector2 direction =
            (boss.Player.position - transform.position).normalized;

        float distance =
            Vector2.Distance(
                transform.position,
                boss.Player.position
            );

        float time = 0f;
        float dashTime = distance / dashSpeed;

        boss.PlayAttackMotion(3);

        StartCoroutine(PlayEffect());

        while (time < dashTime)
        {
            time += Time.deltaTime;

            rigid.linearVelocity =
                direction * dashSpeed;

            yield return null;
        }

        rigid.linearVelocity = Vector2.zero;

        boss.SetCanMove(true);
        isAttacking = false;
    }

    IEnumerator PlayEffect()
    {
        if (dashEffects == null ||
            dashEffects.Length == 0 ||
            effectRenderer == null)
            yield break;

        effectRenderer.enabled = true;

        for (int i = 0; i < dashEffects.Length; i++)
        {
            effectRenderer.sprite = dashEffects[i];

            yield return new WaitForSeconds(effectSpeed);
        }

        effectRenderer.enabled = false;
    }
}
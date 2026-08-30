using System.Collections;
using UnityEngine;

public class BossTripleDashPattern : MonoBehaviour
{
    public float dashSpeed = 18f;
    public float dashDistance = 2f;
    public float dashDelay = 0.2f;

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
        if (patternID == 2 && !isAttacking)
            StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        boss.SetCanMove(false);

        for (int i = 0; i < 3; i++)
        {
            if (boss.Player == null)
                break;

            Vector2 direction =
                (boss.Player.position - transform.position).normalized;

            boss.PlayAttackMotion(2);

            StartCoroutine(PlayEffect());

            float time = 0f;
            float dashTime = dashDistance / dashSpeed;

            while (time < dashTime)
            {
                time += Time.deltaTime;

                rigid.linearVelocity = new Vector2(
                    direction.x * dashSpeed,
                    rigid.linearVelocity.y
                );

                yield return null;
            }

            rigid.linearVelocity = new Vector2(
                0f,
                rigid.linearVelocity.y
            );

            if (i < 2)
                yield return new WaitForSeconds(dashDelay);
        }

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
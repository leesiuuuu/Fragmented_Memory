using System.Collections;
using UnityEngine;

public class BossSlashPattern : MonoBehaviour
{
    public float attackDelay = 0.2f;
    public float dashSpeed = 7f;
    public float dashDistance = 2f;
    public float slashDelay = 0.1f;

    [Header("Effect")]
    public Sprite[] slashEffects;
    public float effectSpeed = 0.05f;
    public SpriteRenderer effectRenderer;

    private BossControl boss;
    private Rigidbody2D rb;
    private bool isAttacking;

    void Awake()
    {
        boss = GetComponent<BossControl>();
        rb = GetComponent<Rigidbody2D>();

        if (effectRenderer != null)
            effectRenderer.enabled = false;
    }

    void OnEnable()
    {
        if (boss == null)
            boss = GetComponent<BossControl>();

        boss.OnPatternSelected += PatternSelected;
    }

    void OnDisable()
    {
        if (boss != null)
            boss.OnPatternSelected -= PatternSelected;

        if (effectRenderer != null)
            effectRenderer.enabled = false;
    }

    void PatternSelected(int patternID, float attackPower)
    {
        if (patternID != 1)
            return;

        if (isAttacking)
            return;

        StartCoroutine(SlashAttack());
    }

    IEnumerator SlashAttack()
    {
        isAttacking = true;
        boss.SetCanMove(false);

        if (boss.Player == null)
        {
            boss.SetCanMove(true);
            isAttacking = false;
            yield break;
        }

        yield return new WaitForSeconds(attackDelay);

        Vector2 direction =
            (boss.Player.position - transform.position).normalized;

        yield return StartCoroutine(Dash(direction));

        if (boss.Player != null)
        {
            direction =
                (boss.Player.position - transform.position).normalized;
        }

        boss.PlayAttackMotion(1);
        StartCoroutine(PlaySlashEffect());

        yield return new WaitForSeconds(slashDelay);

        boss.PlayAttackMotion(1);
        StartCoroutine(PlaySlashEffect());

        yield return new WaitForSeconds(0.3f);

        boss.SetCanMove(true);
        isAttacking = false;
    }

    IEnumerator Dash(Vector2 direction)
    {
        float time = 0f;
        float dashTime = dashDistance / dashSpeed;

        while (time < dashTime)
        {
            time += Time.deltaTime;

            rb.linearVelocity = new Vector2(
                direction.x * dashSpeed,
                rb.linearVelocity.y
            );

            yield return null;
        }

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    IEnumerator PlaySlashEffect()
    {
        if (slashEffects == null ||
            slashEffects.Length == 0 ||
            effectRenderer == null)
            yield break;

        effectRenderer.enabled = true;

        for (int i = 0; i < slashEffects.Length; i++)
        {
            effectRenderer.sprite = slashEffects[i];

            yield return new WaitForSeconds(effectSpeed);
        }

        effectRenderer.enabled = false;
    }
}
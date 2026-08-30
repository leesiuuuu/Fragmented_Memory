using System.Collections;
using UnityEngine;

public class BossSwordWavePattern : MonoBehaviour
{
    public GameObject wavePrefab;

    public float chargeTime = 0.7f;
    public float waveSpeed = 15f;
    public float waveLifeTime = 5f;

    [Header("Effect")]
    public Sprite[] chargeEffects;
    public float effectSpeed = 0.05f;
    public SpriteRenderer effectRenderer;

    private BossControl boss;
    private bool isAttacking;

    void Awake()
    {
        boss = GetComponent<BossControl>();

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
        if (patternID == 4 && !isAttacking)
            StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        boss.SetCanMove(false);

        boss.PlayAttackMotion(4);

        StartCoroutine(PlayEffect());

        yield return new WaitForSeconds(chargeTime);

        if (boss.Player == null)
        {
            boss.SetCanMove(true);
            isAttacking = false;
            yield break;
        }

        Vector2 direction =
            (boss.Player.position - transform.position).normalized;

        if (wavePrefab == null)
        {
            boss.SetCanMove(true);
            isAttacking = false;
            yield break;
        }

        GameObject wave =
            Instantiate(
                wavePrefab,
                transform.position,
                Quaternion.identity
            );

        Rigidbody2D rigid =
            wave.GetComponent<Rigidbody2D>();

        if (rigid != null)
            rigid.linearVelocity =
                direction * waveSpeed;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        wave.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        Destroy(wave, waveLifeTime);

        boss.SetCanMove(true);
        isAttacking = false;
    }

    IEnumerator PlayEffect()
    {
        if (chargeEffects == null ||
            chargeEffects.Length == 0 ||
            effectRenderer == null)
            yield break;

        effectRenderer.enabled = true;

        for (int i = 0; i < chargeEffects.Length; i++)
        {
            effectRenderer.sprite = chargeEffects[i];

            yield return new WaitForSeconds(effectSpeed);
        }

        effectRenderer.enabled = false;
    }
}
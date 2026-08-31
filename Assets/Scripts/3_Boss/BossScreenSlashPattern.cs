using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossScreenSlashPattern : MonoBehaviour
{
    [Header("Jump")]
    public float jumpPower = 5f;
    public float jumpTime = 0.25f;

    [Header("Screen Image")]
    public Canvas canvas;
    public Image screenImage;
    public float imageTime = 0.2f;

    [Header("Slash")]
    public float chargeTime = 0.5f;
    public float slashTime = 0.35f;
    public float slashHeight = 20f;
    public float slashWidth = 0.5f;
    public float slashAngle = 45f;
    public LineRenderer slashLine;

    [Header("Damage")]
    public float damage = 10f;
    public float hitRadius = 0.5f;
    public LayerMask playerLayer;

    private BossControl boss;
    private Rigidbody2D rigid;

    private bool isAttacking;

    private float startWidth;
    private float endWidth;
    private Color startColor;
    private Color endColor;

    void Awake()
    {
        boss = GetComponent<BossControl>();
        rigid = GetComponent<Rigidbody2D>();

        if (slashLine != null)
        {
            startWidth = slashLine.startWidth;
            endWidth = slashLine.endWidth;

            startColor = slashLine.startColor;
            endColor = slashLine.endColor;

            slashLine.enabled = false;
        }

        if (screenImage != null)
            screenImage.gameObject.SetActive(false);
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

        if (slashLine != null)
            slashLine.enabled = false;

        if (screenImage != null)
            screenImage.gameObject.SetActive(false);
    }

    void Pattern(int patternID, float attackPower)
    {
        if (patternID == 5 && !isAttacking)
        {
            damage = attackPower;
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        boss.SetCanMove(false);

        if (boss.Player == null)
        {
            boss.SetCanMove(true);
            isAttacking = false;
            yield break;
        }

        if (rigid != null)
        {
            rigid.linearVelocity = new Vector2(
                rigid.linearVelocity.x,
                jumpPower
            );

            yield return new WaitForSeconds(jumpTime);

            rigid.linearVelocity = new Vector2(
                rigid.linearVelocity.x,
                0f
            );
        }

        Vector3 target = boss.Player.position;

        ShowScreenImage();

        yield return new WaitForSeconds(imageTime);

        HideScreenImage();

        yield return new WaitForSeconds(chargeTime);

        ShowSlash(target);

        boss.PlayAttackMotion(5);

        DamagePlayerAtPosition(target);

        yield return new WaitForSeconds(slashTime);

        HideSlash();

        boss.SetCanMove(true);

        isAttacking = false;
    }

    void DamagePlayerAtPosition(Vector3 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(
            position,
            hitRadius,
            playerLayer
        );

        if (hit == null)
            return;

        if (boss.Player == null)
            return;

        if (hit.transform == boss.Player ||
            hit.transform.IsChildOf(boss.Player))
        {
            boss.DamagePlayer(damage);
        }
    }

    void ShowScreenImage()
    {
        if (screenImage == null)
            return;

        screenImage.gameObject.SetActive(true);

        RectTransform imageRect =
            screenImage.rectTransform;

        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;

        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
    }

    void HideScreenImage()
    {
        if (screenImage == null)
            return;

        screenImage.gameObject.SetActive(false);
    }

    void ShowSlash(Vector3 target)
    {
        if (slashLine == null)
            return;

        Vector3 direction =
            Quaternion.Euler(
                0f,
                0f,
                -slashAngle
            ) * Vector3.up;

        slashLine.positionCount = 2;

        slashLine.startWidth = slashWidth;
        slashLine.endWidth = slashWidth;

        slashLine.SetPosition(
            0,
            target + direction * slashHeight
        );

        slashLine.SetPosition(
            1,
            target - direction * slashHeight
        );

        Color purple = new Color(
            0.7f,
            0.2f,
            1f,
            1f
        );

        slashLine.startColor = purple;
        slashLine.endColor = purple;

        slashLine.enabled = true;
    }

    void HideSlash()
    {
        if (slashLine == null)
            return;

        slashLine.enabled = false;

        slashLine.startWidth = startWidth;
        slashLine.endWidth = endWidth;

        slashLine.startColor = startColor;
        slashLine.endColor = endColor;
    }
}
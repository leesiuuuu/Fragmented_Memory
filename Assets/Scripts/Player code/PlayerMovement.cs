using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    PlayerHP playerHP;
    PlayerCombat combat;
    PlayerInvincibility invincibility;


    int jumpCount = 0;
    int additionalJumpCount;
    int maxDashCount = 1;
    int dashCount = 1;


    public float moveSpeed = 9f;
    public float jumpPower = 8f;
    public float dashPower = 13f;
    [FormerlySerializedAs("dashCoolTime")]
    public float dashDuration = 0.16f;
    [FormerlySerializedAs("dashTime")]
    public float dashCooldown = 0.7f;
    public float dashFallSpeed = 3f;


    bool isDash = false;

    bool isGround = false;
    float normalGravityScale;
    float externalMovementMultiplier = 1f;
    float? forcedHorizontalSpeed;

    public float CurrentMoveSpeed => moveSpeed * externalMovementMultiplier;
    public event System.Action MovementSpeedChanged;



    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerHP = GetComponent<PlayerHP>();
        combat = GetComponent<PlayerCombat>();
        invincibility = GetComponent<PlayerInvincibility>();
        normalGravityScale = rigid.gravityScale;
    }



    void Update()
    {
        if (playerHP.IsDead || GameplayInputLock.IsLocked)
            return;


        float h = Input.GetAxisRaw("Horizontal");


        if (!combat.IsBusy && h < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (!combat.IsBusy && h > 0)
        {
            spriteRenderer.flipX = false;
        }



        // 점프

        if ((Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.UpArrow))
            && !combat.IsBusy
            && !isDash
            && jumpCount < 2 + additionalJumpCount)
        {

            rigid.linearVelocity =
                new Vector2(
                    rigid.linearVelocity.x,
                    0
                );


            rigid.AddForce(
                Vector2.up * jumpPower * externalMovementMultiplier,
                ForceMode2D.Impulse
            );


            jumpCount++;

            isGround = false;

            if(EffectManager.Instance != null)
            {
                EffectManager.Instance.Play(
                    EffectId.Jump,
                    transform.position,
                    Quaternion.identity
                );
            }
        }



        // 대쉬

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCount > 0)
        {
            dashCount--;
            isDash = true;
            invincibility?.StartDashInvincibility();

            float direction = spriteRenderer.flipX ? -1 : 1;

            rigid.linearVelocity =
                new Vector2(
                    direction * dashPower * externalMovementMultiplier,
                    0f
                );

            rigid.gravityScale = 0f;

            Invoke(nameof(EndDash), dashDuration);
            Invoke(nameof(ResetDash), dashCooldown);
        }
    }




    void FixedUpdate()
    {
        if(playerHP.IsDead)
        {
            rigid.linearVelocity = Vector2.zero;

            animator.SetFloat("move speed", 0);
            animator.SetFloat("jump power", 0);

            return;
        }

        if (GameplayInputLock.IsLocked)
        {
            rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
            animator.SetFloat("move speed", 0f);
            return;
        }



        float h = Input.GetAxisRaw("Horizontal");


        if(!isDash)
        {
            rigid.linearVelocity =
                new Vector2(
                    forcedHorizontalSpeed
                        ?? (combat.IsBusy
                            ? 0f
                            : h * moveSpeed * externalMovementMultiplier),
                    rigid.linearVelocity.y
                );
        }



        animator.SetFloat(
            "move speed",
            Mathf.Abs(rigid.linearVelocity.x)
        );


        animator.SetFloat(
            "jump power",
            rigid.linearVelocity.y
        );


        animator.SetBool(
            "isGround",
            isGround
        );
    }




    void EndDash()
    {
        isDash = false;
        invincibility?.EndDashInvincibility();
        rigid.gravityScale = normalGravityScale;
        rigid.linearVelocity = new Vector2(
            rigid.linearVelocity.x,
            -Mathf.Abs(dashFallSpeed)
        );
    }



    void ResetDash()
    {
        dashCount = Mathf.Min(dashCount + 1, maxDashCount);

        if (dashCount < maxDashCount)
            Invoke(nameof(ResetDash), dashCooldown);
    }


    public void SetExternalMovementMultiplier(float multiplier)
    {
        float nextMultiplier = Mathf.Max(0f, multiplier);

        if (Mathf.Approximately(externalMovementMultiplier, nextMultiplier))
            return;

        externalMovementMultiplier = nextMultiplier;
        MovementSpeedChanged?.Invoke();
    }


    public void AddMaxJumpCount(int amount)
    {
        if (amount > 0)
            additionalJumpCount += amount;
    }


    public void AddMaxDashCount(int amount)
    {
        int addedCount = Mathf.Max(0, amount);
        maxDashCount += addedCount;
        dashCount += addedCount;
    }


    public void SetForcedHorizontalSpeed(float speed)
    {
        forcedHorizontalSpeed = speed;
    }


    public void ClearForcedHorizontalSpeed()
    {
        forcedHorizontalSpeed = null;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(EndDash));
        CancelInvoke(nameof(ResetDash));

        isDash = false;
        dashCount = maxDashCount;
        invincibility?.EndDashInvincibility();

        if (rigid != null)
            rigid.gravityScale = normalGravityScale;
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
            isGround = true;
        }
    }



    void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }
}

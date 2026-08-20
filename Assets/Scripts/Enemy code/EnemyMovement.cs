using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;
    private PlayerHP playerHP;
    private EnemyAttack enemyAttack;

    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int DashAttack = Animator.StringToHash("DashAttack");

    [Header("추적")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 1.3f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private bool invertFacing;

    [Header("공중 이동")]
    [SerializeField] private bool useFlyingMovement;
    [SerializeField] private float flyingHeight = 0.5f;
    [SerializeField] private float takeoffSpeed = 1.5f;
    [SerializeField] private float landingSpeed = 1.2f;

    [Header("점프 이동")]
    [SerializeField] private bool useHopMovement;
    [SerializeField] private float hopPower = 3f;
    [SerializeField] private float hopCooldown = 0.7f;

    [Header("점프 돌진")]
    [SerializeField] private bool useJumpCharge;
    [SerializeField] private float jumpTriggerDistance = 4f;
    [SerializeField] private float jumpHorizontalSpeed = 6f;
    [SerializeField] private float jumpPower = 8f;
    [SerializeField] private float jumpCooldown = 3f;

    [Header("지상 돌진")]
    [SerializeField] private bool useGroundDash;
    [SerializeField] private float dashTriggerDistance = 3f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.35f;
    [SerializeField] private float dashCooldown = 2.5f;

    private bool isGrounded;
    private bool isJumpCharging;
    private bool leftGroundAfterJump;
    private bool isDashing;
    private float nextJumpTime;
    private float nextDashTime;
    private float dashEndTime;
    private float dashDirection;
    private float nextHopTime;
    private bool isFlying;
    private float groundHeight;
    private float originalGravityScale;
    private readonly HashSet<Collider2D> groundColliders = new HashSet<Collider2D>();

    public bool IsSpecialMoving => isJumpCharging || isDashing;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyAttack = GetComponent<EnemyAttack>();
        originalGravityScale = rigid.gravityScale;
    }


    private void Start()
    {
        FindPlayer();

    }


    private void Update()
    {
        if (player == null)
            return;

        if (playerHP != null && playerHP.IsDead)
        {
            animator.SetFloat(MoveSpeed, 0f);
            return;
        }

        Flip();
        animator.SetFloat(MoveSpeed, Mathf.Abs(rigid.linearVelocity.x));
    }


    private void FixedUpdate()
    {
        if (player == null)
            return;

        if (playerHP != null && playerHP.IsDead)
        {
            StopHorizontalMovement();
            UpdateFlyingMovement(false);
            return;
        }

        if (enemyAttack != null && enemyAttack.IsStationaryAttacking)
        {
            StopHorizontalMovement();
            UpdateFlyingMovement(false);
            return;
        }

        if (isJumpCharging)
        {
            UpdateJumpCharge();
            return;
        }

        if (isDashing)
        {
            UpdateGroundDash();
            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();

        if (TryStartJumpCharge(distanceToPlayer) || TryStartGroundDash(distanceToPlayer))
            return;

        UpdateTracking(distanceToPlayer);
    }


    private void FindPlayer()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");

        if (target != null)
        {
            player = target.transform;
            playerHP = target.GetComponent<PlayerHP>();
        }
    }


    public float GetDistanceToPlayer()
    {
        if (player == null)
            return Mathf.Infinity;

        return Vector2.Distance(transform.position, player.position);
    }


    private void UpdateTracking(float distanceToPlayer)
    {
        if (distanceToPlayer <= stoppingDistance || distanceToPlayer >= detectionRange)
        {
            StopHorizontalMovement();
            UpdateFlyingMovement(false);
            return;
        }

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        UpdateFlyingMovement(true);

        if (useHopMovement)
        {
            if (!isGrounded || Time.time < nextHopTime)
                return;

            nextHopTime = Time.time + hopCooldown;
            rigid.linearVelocity = new Vector2(direction * moveSpeed, hopPower);
            animator.SetTrigger(Jump);
            return;
        }

        rigid.linearVelocity = new Vector2(direction * moveSpeed, rigid.linearVelocity.y);
    }


    private bool TryStartJumpCharge(float distanceToPlayer)
    {
        if (!useJumpCharge || !isGrounded || Time.time < nextJumpTime)
            return false;

        if (distanceToPlayer > jumpTriggerDistance)
            return false;

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        isJumpCharging = true;
        leftGroundAfterJump = false;
        nextJumpTime = Time.time + jumpCooldown;
        rigid.linearVelocity = new Vector2(direction * jumpHorizontalSpeed, jumpPower);
        animator.SetTrigger(Jump);
        enemyAttack?.BeginJumpAttack();

        return true;
    }


    private void UpdateJumpCharge()
    {
        if (!isGrounded)
            leftGroundAfterJump = true;

        if (!leftGroundAfterJump || !isGrounded || rigid.linearVelocity.y > 0f)
            return;

        isJumpCharging = false;
        StopHorizontalMovement();
        enemyAttack?.EndJumpAttack();
    }


    private bool TryStartGroundDash(float distanceToPlayer)
    {
        if (!useGroundDash || Time.time < nextDashTime)
            return false;

        if (distanceToPlayer > dashTriggerDistance)
            return false;

        dashDirection = Mathf.Sign(player.position.x - transform.position.x);
        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;
        animator.SetTrigger(DashAttack);
        enemyAttack?.BeginDashAttack();

        return true;
    }


    private void UpdateGroundDash()
    {
        if (Time.time < dashEndTime)
        {
            rigid.linearVelocity = new Vector2(dashDirection * dashSpeed, rigid.linearVelocity.y);
            return;
        }

        isDashing = false;
        StopHorizontalMovement();
        enemyAttack?.EndDashAttack();
    }


    private void StopHorizontalMovement()
    {
        rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
    }


    private void UpdateFlyingMovement(bool shouldFly)
    {
        if (!useFlyingMovement)
            return;

        if (shouldFly)
        {
            if (!isFlying)
            {
                isFlying = true;
                groundHeight = rigid.position.y;
                rigid.gravityScale = 0f;
            }

            MoveVertically(groundHeight + flyingHeight, takeoffSpeed);
            return;
        }

        if (!isFlying)
            return;

        if (Mathf.Abs(rigid.position.y - groundHeight) <= landingSpeed * Time.fixedDeltaTime)
        {
            rigid.position = new Vector2(rigid.position.x, groundHeight);
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
            rigid.gravityScale = originalGravityScale;
            isFlying = false;
            return;
        }

        MoveVertically(groundHeight, landingSpeed);
    }


    private void MoveVertically(float targetHeight, float speed)
    {
        float distance = targetHeight - rigid.position.y;
        float verticalSpeed = Mathf.Clamp(
            distance / Time.fixedDeltaTime,
            -speed,
            speed);
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, verticalSpeed);
    }


    private void Flip()
    {
        bool faceRight = isDashing
            ? dashDirection > 0f
            : player.position.x >= transform.position.x;

        spriteRenderer.flipX = invertFacing ? !faceRight : faceRight;
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        if (HasGroundContact(collision))
            groundColliders.Add(collision.collider);

        isGrounded = groundColliders.Count > 0;
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        groundColliders.Remove(collision.collider);
        isGrounded = groundColliders.Count > 0;
    }


    private bool HasGroundContact(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
                return true;
        }

        return false;
    }


    private void OnDisable()
    {
        if (useFlyingMovement)
            rigid.gravityScale = originalGravityScale;
    }
}

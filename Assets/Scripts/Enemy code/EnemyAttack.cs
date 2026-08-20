using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyMovement movement;
    private Animator animator;
    private Rigidbody2D rigid;
    private Collider2D ownerCollider;
    private Transform player;
    private PlayerHP playerHP;

    private static readonly int RangedAttack = Animator.StringToHash("RangedAttack");
    private static readonly int MeleeAttack = Animator.StringToHash("MeleeAttack");
    private static readonly int Landing = Animator.StringToHash("Landing");

    [Header("공격 전 착지")]
    [SerializeField] private bool useLandingBeforeAttack;
    [SerializeField] private float landingDuration = 0.417f;

    [Header("접촉 공격")]
    [SerializeField] private bool useContactAttack;
    [SerializeField] private float contactAttackCooldown = 2f;

    [Header("특수 이동 공격")]
    [SerializeField] private bool useJumpImpactAttack;
    [SerializeField] private bool useDashImpactAttack;

    [Header("원거리 공격")]
    [SerializeField] private bool useRangedAttack;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float rangedAttackMinDistance = 3f;
    [SerializeField] private float rangedAttackRange = 6f;
    [SerializeField] private float rangedAttackCooldown = 3f;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileLifetime = 4f;
    [SerializeField] private float projectileMaxDistance;
    [SerializeField] private float projectileHomingDuration;
    [SerializeField] private float projectileDamageMultiplier = 0.5f;
    [SerializeField] private float throwDelay = 0.25f;
    [SerializeField] private float throwAnimationDuration = 0.333f;

    [Header("범위 공격")]
    [SerializeField] private bool useAreaAttack;
    [SerializeField] private EnemyGasArea areaPrefab;
    [SerializeField] private Transform areaPoint;
    [SerializeField] private float areaAttackRange = 3f;
    [SerializeField] private float areaAttackCooldown = 4f;
    [SerializeField] private float areaDuration = 2f;
    [SerializeField] private float areaDamageInterval = 0.5f;
    [SerializeField] private float areaTravelDistance = 2f;
    [SerializeField] private float areaTravelDuration = 0.25f;
    [SerializeField] private float areaAttackDelay = 0.167f;
    [SerializeField] private float areaAttackAnimationDuration = 0.333f;

    [Header("근접 공격")]
    [SerializeField] private bool useMeleeAttack;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float meleeAttackRange = 1.5f;
    [SerializeField] private float meleeAttackCooldown = 4f;
    [SerializeField] private float meleeAttackDelay = 0.167f;
    [SerializeField] private float meleeAttackAnimationDuration = 0.25f;

    private bool jumpAttackActive;
    private bool jumpAttackHit;
    private bool dashAttackActive;
    private bool dashAttackHit;
    private float nextContactAttackTime;
    private float nextRangedAttackTime;
    private float nextAreaAttackTime;
    private float nextMeleeAttackTime;
    private bool isThrowing;
    private bool isAreaAttacking;
    private bool isMeleeAttacking;
    private bool isLandingForAttack;
    private bool stoppedForPlayerDeath;
    private readonly List<Collider2D> meleeHits = new List<Collider2D>(16);
    private ContactFilter2D attackFilter;

    public bool IsStationaryAttacking => isThrowing || isAreaAttacking || isMeleeAttacking
        || isLandingForAttack;


    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        movement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        ownerCollider = GetComponent<Collider2D>();
        attackFilter.NoFilter();
    }


    private void Start()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");

        if (target != null)
        {
            player = target.transform;
            playerHP = target.GetComponent<PlayerHP>();
        }
    }


    private void Update()
    {
        if (playerHP != null && playerHP.IsDead)
        {
            if (!stoppedForPlayerDeath)
                StopForPlayerDeath();

            return;
        }

        if (player == null || movement.IsSpecialMoving || IsStationaryAttacking)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (TryStartRangedAttack(distance))
            return;

        if (TryStartAreaAttack(distance))
            return;

        TryStartMeleeAttack(distance);
    }


    public void BeginJumpAttack()
    {
        if (!useJumpImpactAttack)
            return;

        jumpAttackActive = true;
        jumpAttackHit = false;
    }


    public void EndJumpAttack()
    {
        jumpAttackActive = false;
    }


    public void BeginDashAttack()
    {
        if (!useDashImpactAttack)
            return;

        dashAttackActive = true;
        dashAttackHit = false;
    }


    public void EndDashAttack()
    {
        dashAttackActive = false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }


    private void TryDamagePlayer(Collider2D other)
    {
        PlayerHP playerHP = other.GetComponentInParent<PlayerHP>();

        if (playerHP == null || playerHP.IsDead)
            return;

        if (jumpAttackActive && !jumpAttackHit)
        {
            jumpAttackHit = true;
            playerHP.TakeDamage(stats.attack);
            return;
        }

        if (dashAttackActive && !dashAttackHit)
        {
            dashAttackHit = true;
            playerHP.TakeDamage(stats.attack);
            return;
        }

        if (useContactAttack && Time.time >= nextContactAttackTime)
        {
            nextContactAttackTime = Time.time + contactAttackCooldown;
            playerHP.TakeDamage(stats.attack);
        }
    }


    private void ThrowProjectile()
    {
        nextRangedAttackTime = Time.time + rangedAttackCooldown;

        if (projectilePrefab == null || throwPoint == null)
            return;

        isThrowing = true;

        if (TryStartLanding(nameof(StartThrowAnimation)))
            return;

        StartThrowAnimation();
    }


    private bool TryStartRangedAttack(float distance)
    {
        if (!useRangedAttack || Time.time < nextRangedAttackTime)
            return false;

        if (distance < rangedAttackMinDistance || distance > rangedAttackRange)
            return false;

        ThrowProjectile();
        return true;
    }


    private bool TryStartAreaAttack(float distance)
    {
        if (!useAreaAttack || Time.time < nextAreaAttackTime || distance > areaAttackRange)
            return false;

        nextAreaAttackTime = Time.time + areaAttackCooldown;

        if (areaPrefab == null || areaPoint == null)
            return false;

        isAreaAttacking = true;
        animator.SetTrigger(RangedAttack);
        Invoke(nameof(SpawnArea), areaAttackDelay);
        Invoke(nameof(FinishAreaAttack), areaAttackAnimationDuration);
        return true;
    }


    private bool TryStartMeleeAttack(float distance)
    {
        if (!useMeleeAttack || Time.time < nextMeleeAttackTime || distance > meleeAttackRange)
            return false;

        nextMeleeAttackTime = Time.time + meleeAttackCooldown;

        if (attackPoint == null)
            return false;

        isMeleeAttacking = true;

        if (TryStartLanding(nameof(StartMeleeAnimation)))
            return true;

        StartMeleeAnimation();
        return true;
    }


    private bool TryStartLanding(string nextAction)
    {
        if (!useLandingBeforeAttack || Mathf.Abs(rigid.linearVelocity.x) < 0.01f)
            return false;

        isLandingForAttack = true;
        animator.SetTrigger(Landing);
        Invoke(nextAction, landingDuration);
        return true;
    }


    private void StartThrowAnimation()
    {
        isLandingForAttack = false;
        animator.SetTrigger(RangedAttack);
        Invoke(nameof(SpawnProjectile), throwDelay);
        Invoke(nameof(FinishThrowAttack), throwAnimationDuration);
    }


    private void StartMeleeAnimation()
    {
        isLandingForAttack = false;
        animator.SetTrigger(MeleeAttack);
        Invoke(nameof(ApplyMeleeDamage), meleeAttackDelay);
        Invoke(nameof(FinishMeleeAttack), meleeAttackAnimationDuration);
    }


    private void SpawnProjectile()
    {
        if (player == null || playerHP == null || playerHP.IsDead)
            return;

        Vector2 direction = (player.position - transform.position).normalized;
        Vector3 spawnPosition = throwPoint.position;
        spawnPosition.x = transform.position.x
            + Mathf.Sign(direction.x) * Mathf.Abs(throwPoint.localPosition.x);
        int damage = Mathf.RoundToInt(stats.attack * projectileDamageMultiplier);

        EnemyProjectile projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity);

        projectile.Initialize(
            damage,
            direction,
            projectileSpeed,
            projectileLifetime,
            ownerCollider,
            player,
            projectileHomingDuration,
            projectileMaxDistance);
    }


    private void SpawnArea()
    {
        if (player == null || playerHP == null || playerHP.IsDead)
            return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Vector3 spawnPosition = areaPoint.position;
        spawnPosition.x = transform.position.x
            + direction * Mathf.Abs(areaPoint.localPosition.x);

        EnemyGasArea area = Instantiate(areaPrefab, spawnPosition, Quaternion.identity);
        area.Initialize(
            stats.attack,
            areaDuration,
            areaDamageInterval,
            direction < 0f,
            direction,
            areaTravelDistance,
            areaTravelDuration);
    }


    private void ApplyMeleeDamage()
    {
        if (player == null || playerHP == null || playerHP.IsDead)
            return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Vector2 center = attackPoint.position;
        center.x = transform.position.x
            + direction * Mathf.Abs(attackPoint.localPosition.x);

        meleeHits.Clear();
        Physics2D.OverlapCircle(center, meleeAttackRange, attackFilter, meleeHits);

        foreach (Collider2D hit in meleeHits)
        {
            PlayerHP playerHP = hit.GetComponentInParent<PlayerHP>();

            if (playerHP == null)
                continue;

            playerHP.TakeDamage(stats.attack);
            return;
        }
    }


    private void FinishThrowAttack()
    {
        isThrowing = false;
    }


    private void FinishAreaAttack()
    {
        isAreaAttacking = false;
    }


    private void FinishMeleeAttack()
    {
        isMeleeAttacking = false;
    }


    private void StopForPlayerDeath()
    {
        stoppedForPlayerDeath = true;
        CancelInvoke();
        isThrowing = false;
        isAreaAttacking = false;
        isMeleeAttacking = false;
        isLandingForAttack = false;
    }


    private void OnDisable()
    {
        CancelInvoke();
        isThrowing = false;
        isAreaAttacking = false;
        isMeleeAttacking = false;
        isLandingForAttack = false;
        stoppedForPlayerDeath = false;
    }


    private void OnDrawGizmosSelected()
    {
        if (useMeleeAttack && attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, meleeAttackRange);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private const float AttackBoxDuration = 0.25f;

    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private PlayerHP playerHP;
    private SpriteRenderer spriteRenderer;
    private PlayerSynergyManager synergyManager;

    [Header("입력")]
    [SerializeField] private KeyCode attackKey = KeyCode.D;

    [Header("Attack Box")]
    [SerializeField] private Transform attackBox;
    [SerializeField] private GameObject attackBoxPrefab;
    [SerializeField] private float attackBoxOffset = 1f;

    [Header("Attack")]
    [SerializeField] private float attackCoolTime = 0.5f;

    [Header("전진 연출")]
    // 콤보 단계별 전진. 3타가 크게 튀어나가야 마무리가 산다.
    // 배열이 콤보 수보다 짧으면 마지막 값을 쓴다.
    [SerializeField]
    private ComboStep[] comboSteps =
    {
        new ComboStep { speed = 2.5f, duration = 0.10f },
        new ComboStep { speed = 3f,   duration = 0.10f },
        new ComboStep { speed = 5f,   duration = 0.16f }
    };

    [Header("타격 연출")]
    // 0이면 끈다. 평타는 가볍게 — 스킬보다 무거우면 타격의 서열이 뒤집힌다.
    [SerializeField, Min(0f)] private float hitStopDuration = HitStop.Light;
    [SerializeField, Min(0f)] private float hitShakeStrength = 0.1f;
    [SerializeField, Min(0f)] private float hitShakeDuration = 0.12f;

    // 값 두 개를 배열 두 개로 나눠 두면 인스펙터에서 짝이 어긋난다.
    [System.Serializable]
    private struct ComboStep
    {
        // 유닛/초. 바라보는 방향으로 나간다.
        public float speed;

        public float duration;
    }

    private PlayerMovement movement;
    private Coroutine stepRoutine;

    private int attackCombo = 0;
    private bool comboQueued = false;
    private bool canAttack = true;
    private bool isAttacking = false;
    private float attackCooldownMultiplier = 1f;
    private Coroutine attackCooldownRoutine;

    private readonly HashSet<EnemyHP> hitEnemies =
        new HashSet<EnemyHP>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        playerHP = GetComponent<PlayerHP>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        synergyManager = GetComponent<PlayerSynergyManager>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (playerHP.IsDead ||
            GameplayInputLock.IsLocked)
            return;

        UpdateAttackBoxDirection();
        AttackInput();
    }

    private void UpdateAttackBoxDirection()
    {
        if (spriteRenderer.flipX)
        {
            attackBox.localPosition =
                new Vector2(
                    -attackBoxOffset,
                    attackBox.localPosition.y
                );
        }
        else
        {
            attackBox.localPosition =
                new Vector2(
                    attackBoxOffset,
                    attackBox.localPosition.y
                );
        }
    }

    private void AttackInput()
    {
        if (Input.GetKeyDown(attackKey))
        {
            if (!combat.IsBusy && canAttack)
            {
                StartAttack();
            }
            else if (isAttacking)
            {
                comboQueued = true;
            }
        }
    }

    private void StartAttack()
    {
        combat.StartAction();

        canAttack = false;

        comboQueued = false;
        isAttacking = true;

        attackCombo = 1;

        animator.SetInteger(
            "AttackCombo",
            attackCombo
        );

        animator.SetTrigger("Attack");

        StepForward(attackCombo);
    }

    public void Damage()
    {
        if (playerHP.IsDead)
            return;

        BoxCollider2D attackCollider =
            SpawnAttackBox();

        if (attackCollider == null)
            return;

        Collider2D[] enemies =
            Physics2D.OverlapBoxAll(
                attackCollider.bounds.center,
                attackCollider.bounds.size,
                0f
            );

        hitEnemies.Clear();

        foreach (Collider2D enemy in enemies)
        {
            EnemyHP enemyHP =
                enemy.GetComponentInParent<EnemyHP>();

            if (enemyHP == null)
                continue;

            if (!hitEnemies.Add(enemyHP))
                continue;

            int damage =
                Mathf.RoundToInt(
                    playerStats.GetAttackDamage() * 0.5f
                );

            int dealtDamage =
                enemyHP.TakeDamage(damage);

            HealFromDamage(dealtDamage);

            synergyManager?.OnDamageDealt(
                enemyHP,
                dealtDamage
            );
        }

        // 한 번의 휘두름에 여럿을 맞혀도 연출은 한 번만 나간다.
        if (hitEnemies.Count > 0)
            PlayHitFeedback();
    }


    // 히트스톱과 흔들림이 같은 순간에 터져야 타격이 하나의 사건으로 읽힌다.
    private void PlayHitFeedback()
    {
        HitStop.Play(hitStopDuration);

        if (CameraFollow.Active != null)
            CameraFollow.Active.Shake(hitShakeStrength, hitShakeDuration);
    }

    private void HealFromDamage(int damage)
    {
        if (damage <= 0 ||
            playerStats.CurrentLifeSteal <= 0f)
            return;

        playerHP.Heal(
            Mathf.RoundToInt(
                damage *
                playerStats.CurrentLifeSteal /
                100f
            )
        );
    }

    public void PlayBasicAttackEffect()
    {
        if (playerHP.IsDead ||
            EffectManager.Instance == null)
            return;

        Quaternion effectRotation =
            spriteRenderer.flipX
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;

        EffectManager.Instance.Play(
            EffectId.BasicAttack,
            attackBox.position,
            effectRotation
        );
    }

    // PlayerMovement가 공격 중 속도를 0으로 묶는데, 강제 속도는 그보다 우선한다.
    // 그래서 별도의 이동 경로를 만들 필요 없이 이 API만으로 전진이 된다.
    private void StepForward(int combo)
    {
        if (movement == null ||
            comboSteps == null ||
            comboSteps.Length == 0)
            return;

        ComboStep step =
            comboSteps[
                Mathf.Clamp(combo - 1, 0, comboSteps.Length - 1)
            ];

        if (step.speed <= 0f || step.duration <= 0f)
            return;

        CancelStep();

        stepRoutine = StartCoroutine(StepRoutine(step));
    }


    private IEnumerator StepRoutine(ComboStep step)
    {
        float direction = spriteRenderer.flipX ? -1f : 1f;

        movement.SetForcedHorizontalSpeed(step.speed * direction);

        // WaitForSeconds는 timeScale을 따르므로 히트스톱이 걸리면 전진도 같이 멈춘다.
        // 화면만 멈추고 몸이 계속 나가면 정지가 어색해지니 이쪽이 맞다.
        yield return new WaitForSeconds(step.duration);

        movement.ClearForcedHorizontalSpeed();

        stepRoutine = null;
    }


    // 공격이 끊기면 강제 속도가 남아 플레이어가 계속 미끄러진다.
    private void CancelStep()
    {
        if (stepRoutine != null)
        {
            StopCoroutine(stepRoutine);
            stepRoutine = null;
        }

        movement?.ClearForcedHorizontalSpeed();
    }


    public void CheckCombo()
    {
        if (comboQueued)
        {
            comboQueued = false;

            attackCombo =
                Mathf.Min(
                    attackCombo + 1,
                    3
                );

            animator.SetInteger(
                "AttackCombo",
                attackCombo
            );

            StepForward(attackCombo);
        }
    }

    public void EndAttack()
    {
        combat.EndAction();

        CancelStep();

        comboQueued = false;
        isAttacking = false;

        attackCombo = 0;

        animator.SetInteger(
            "AttackCombo",
            0
        );

        Invoke(
            nameof(ResetAttackCoolTime),
            attackCoolTime *
            attackCooldownMultiplier
        );
    }

    private BoxCollider2D SpawnAttackBox()
    {
        if (attackBoxPrefab == null)
        {
            Debug.LogError(
                "[PlayerAttack] 일반 공격 판정 프리팹이 연결되지 않았습니다.",
                this
            );

            return null;
        }

        GameObject instance =
            Instantiate(
                attackBoxPrefab,
                attackBox.position,
                Quaternion.identity
            );

        BoxCollider2D collider =
            instance.GetComponent<BoxCollider2D>();

        if (collider == null)
        {
            Debug.LogError(
                "[PlayerAttack] 일반 공격 판정 프리팹에 BoxCollider2D가 없습니다.",
                instance
            );

            Destroy(instance);
            return null;
        }

        SpriteRenderer visual =
            instance.GetComponent<SpriteRenderer>();

        if (visual != null)
        {
            visual.sortingLayerID =
                spriteRenderer.sortingLayerID;

            visual.sortingOrder =
                spriteRenderer.sortingOrder + 1;
        }

        Destroy(
            instance,
            AttackBoxDuration
        );

        return collider;
    }

    public void ApplyAttackCooldownReduction(
        float percent,
        float duration)
    {
        if (attackCooldownRoutine != null)
            StopCoroutine(attackCooldownRoutine);

        attackCooldownRoutine =
            StartCoroutine(
                AttackCooldownEffect(
                    Mathf.Clamp(percent, 0f, 100f),
                    duration
                )
            );
    }

    private IEnumerator AttackCooldownEffect(
        float percent,
        float duration)
    {
        attackCooldownMultiplier =
            1f - percent / 100f;

        yield return new WaitForSeconds(duration);

        attackCooldownMultiplier = 1f;
        attackCooldownRoutine = null;
    }

    private void ResetAttackCoolTime()
    {
        canAttack = true;
    }

    private void OnDisable()
    {
        CancelInvoke(
            nameof(ResetAttackCoolTime)
        );

        CancelStep();

        if (isAttacking)
            combat?.EndAction();

        comboQueued = false;
        isAttacking = false;
        attackCombo = 0;
        canAttack = true;
        attackCooldownMultiplier = 1f;
        attackCooldownRoutine = null;
    }
}
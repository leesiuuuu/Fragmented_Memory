using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MirrorEnemy : Enemy
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private BoxCollider2D checkCollider;

    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Image hpBar;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;

    public int Attacking { get; set; } = 0;
    public int Attackable { get; set; } = 1;
    public bool Groggying { get; set; } = false;



    private Rigidbody2D rb;
    private Transform target;

    private Animator enemyAnimator;

    private float attackCooltime = 1f;

    private void Awake()
    {
        // Rigidbody2D 참조 가져오기
        rb = GetComponent<Rigidbody2D>();
        enemyAnimator = GetComponent<Animator>();
    }

    public override void Start()
    {
        AD += GameManager.Instance.EnemyStat;
        MaxHP += GameManager.Instance.EnemyStat * 5;
        Defence += GameManager.Instance.EnemyStat;

        base.Start();
    }

    private void Update()
    {
        if (IsDeath) return;

        // 감지 범위 내 플레이어 확인
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (playerCollider != null)
        {
            target = playerCollider.transform;
        }
        else
        {
            target = null;
        }
    }

    private void FixedUpdate()
    {
        if (IsDeath) return;
        if (Groggying) return;
        if (target != null)
        {
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance > attackRange)
            {
                MoveToTarget();
            }
            else
            {
                StopMoving();
                if(Attackable == 1 && Attacking == 0)
                    Attack();
            }
        }
        else
        {
            StopMoving();
        }
    }

    public override void Hitted(int value)
    {
        if (!canvas.activeSelf) canvas.SetActive(true);
        base.Hitted(value);
        hpBar.fillAmount = HP / (float)MaxHP;
    }
    protected override void Groggy()
    {
        boxCollider.enabled = false;
        if (!Groggying && target != null) // target(플레이어)이 있을 때 넉백 방향 계산 가능
        {
            // 플레이어와 적의 위치 차이를 이용해 밀려날 방향 계산
            Vector2 knockbackDir = (transform.position - target.position).normalized;
            knockbackDir.y = 0f;
            Debug.Log(knockbackDir);
            StartCoroutine(groggy(knockbackDir));
        }
    }
    protected override void Death()
    {
        if (IsDeath) return; // 중복 실행 방지 (Enemy 클래스에 IsDeath가 있을 경우)

        boxCollider.enabled = false;
        canvas.SetActive(false);
        enemyAnimator.SetTrigger("Death");

        // 1. 죽을 때 넉백 처리
        if (target != null)
        {
            Vector2 dieKnockbackDir = (transform.position - target.position).normalized;
            dieKnockbackDir.y = 0.5f;

            rb.velocity = Vector2.zero;
            rb.AddForce(dieKnockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

        // 2. 물리 및 콜라이더 처리 (지연 처리)
        // 죽자마자 꺼버리면 넉백 힘을 못 받으므로, 콜라이더만 즉시 끄고 Rigidbody는 잠시 둡니다.
        GetComponent<Collider2D>().enabled = false;

        // 0.5초 뒤에 물리 연산을 완전히 멈추도록 코루틴 호출 (선택 사항)
        StartCoroutine(DisablePhysicsAfterDeath());

        base.Death();
    }

    private IEnumerator DisablePhysicsAfterDeath()
    {
        // 넉백으로 날아가는 시간을 잠시 기다림
        yield return new WaitForSeconds(0.5f);

        // 완전히 바닥에 고정하거나 물리 계산을 중단
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        checkCollider.isTrigger = true;
    }

    private IEnumerator groggy(Vector2 direction)
    {
        Groggying = true;

        // 1. 역경직 시작: 애니메이션 멈춤 및 색상 변경
        float originalSpeed = enemyAnimator.speed;
        enemyAnimator.speed = 0;
        GetComponent<SpriteRenderer>().color = Color.black;

        rb.velocity = Vector2.zero;
        rb.AddForce((direction * 0.5f) * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.5f);

        // 4. 복구 로직
        GetComponent<SpriteRenderer>().color = Color.white;
        enemyAnimator.speed = originalSpeed;

        Groggying = false;
    }

    private void MoveToTarget()
    {
        enemyAnimator.SetBool("Moving", true);
        Vector2 direction = (target.position - transform.position).normalized;

        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        // 캔버스의 현재 스케일을 가져오되, X값은 항상 양수(절대값)로 기준을 잡습니다.
        Vector3 cScale = canvas.transform.localScale;
        float baseScaleX = Mathf.Abs(cScale.x);

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            // 오른쪽을 볼 때는 캔버스도 정방향(+)
            canvas.transform.localScale = new Vector3(baseScaleX, cScale.y, cScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            // 왼쪽을 볼 때는 부모가 -1이므로 자식도 -1을 곱해서 상쇄
            canvas.transform.localScale = new Vector3(-baseScaleX, cScale.y, cScale.z);
        }
    }

    private void StopMoving()
    {
        if (Groggying) return;
        // 멈출 때는 속도를 0으로 (y축 값은 중력 유지를 위해 남겨둘 수 있음)
        enemyAnimator.SetBool("Moving", false);
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void Attack()
    {
        enemyAnimator.SetTrigger("Attack");
    }

    public void EnableAttack()
    {
        Attacking = 1;
        Attackable = 0;
        boxCollider.enabled = true;
    }

    public void DisableAttack()
    {
        Attacking = 0;
        boxCollider.enabled = false;
        StartCoroutine(attackCoroutine());
    }

    private IEnumerator attackCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime <= attackCooltime)
        {
            if (IsDeath) yield break;
            if (Groggying) elapsedTime = 0f;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Attackable = 1;
        yield break;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
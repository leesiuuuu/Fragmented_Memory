using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerAnimator animator;
    private Rigidbody2D rigid;
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private GameObject jumpEffect;

    [Header("Ground Check")]
    [SerializeField] private float rayDistance;
    [SerializeField] private LayerMask groundMask;

    [Header("Skill / Dash")]
    [SerializeField] private float skillCooltime = 1f;
    [SerializeField] private float skillDashPower;
    [SerializeField] private float dashCooltime = 0.4f;
    [SerializeField] private float dashPower;
    [SerializeField] private GameObject skillEffect;

    [SerializeField] private Material invertMAT;


    private float skillDuration = 0f;
    private float dashDuration = 0f;
    private float horizontalInput;

    // 플레이어가 공격 가능한지를 나타냄
    public bool Attackable { get; set; } = true;
    // 플레이어가 공격 중인지를 나타냄
    public bool Attacking { get; set; } = false;
    // 플레이어가 스킬 사용 가능한지를 나타냄
    public bool Skillable {  get; set; } = true;
    // 플레이어가 스킬 사용 중인지를 나타냄
    public bool Skilling { get; set; } = false;
    // 플레이어가 대쉬 가능한를 나타냄
    public bool Dashable { get; set; } = true;
    // 플레이어가 대쉬 중인지를 나타냄
    public bool Dashing { get; set; } = false;
    // 플레이어가 잠푸어 할 수 있는지를 나타냄
    public bool Jumpable { get; set; } = false;

    private void Start()
    {
        animator = GetComponent<PlayerAnimator>();
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 좌우 입력
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 공격 관리
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (Dashing) return;
            if (!Attackable || Skilling) return;
            animator.Attack();
        }

        // 점프 관리
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            jump();
        }

        // 스킬 관리
        if (Input.GetKeyDown(KeyCode.A))
        {
            skill();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            dash();
        }

        // 이동 관리
        if (horizontalInput != 0)
        {
            if (!Attackable || Skilling)
            {
                horizontalInput = 0f;
                return;
            }

            transform.localScale = new Vector3(horizontalInput < 0 ? -1f : 1f, 1f, 1f);

            animator.Move(true);
        }
        else
        {
            animator.Move(false);
        }

        // 바닥 관리
        checkGround();
    }

    public void Hitted(int value)
    {
        int final = value - PlayerStatManager.instance.Defence;
        if (final > 0)
        {
            PlayerStatManager.instance.HP -= final;
            StartCoroutine(hitEffect());
        }
    }


    private IEnumerator hitEffect()
    {
        GetComponent<SpriteRenderer>().color = Color.black;

        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().color = Color.white;

    }

    private void FixedUpdate()
    {
        if (Skilling) return;
        if (Dashing)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, 0f);
            return;
        }

        float moveX = horizontalInput * moveSpeed;

        // 공중에 떠 있을 때 벽 끼임 방지 체크
        if (!Jumpable && horizontalInput != 0)
        {
            // 체크할 방향 설정
            Vector2 checkDir = horizontalInput > 0 ? Vector2.right : Vector2.left;

            // 박스 오버랩 범위 설정 (캐릭터 중심에서 약간 앞쪽)
            Vector2 boxSize = new Vector2(0.1f, 0.8f); // 두께는 얇게, 높이는 캐릭터 키에 맞춰 조절
            Vector2 boxCenter = (Vector2)transform.position + (checkDir * 0.2f);

            // 해당 범위에 groundMask 레이어가 있는지 확인
            Collider2D wallHit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundMask);

            // 벽이 감지되면 이동 속도를 0으로 제한
            if (wallHit != null)
            {
                moveX = 0;
            }
        }

        rigid.velocity = new Vector2(moveX, rigid.velocity.y);
    }

    // 에디터 뷰에서 박스 범위를 시각적으로 확인하기 위함
    private void OnDrawGizmos()
    {
        if (horizontalInput == 0) return;

        Gizmos.color = Color.red;
        Vector2 checkDir = horizontalInput > 0 ? Vector2.right : Vector2.left;
        Vector2 boxSize = new Vector2(0.1f, 0.8f);
        Vector2 boxCenter = (Vector2)transform.position + (checkDir * 0.4f);

        Gizmos.DrawWireCube(boxCenter, boxSize);
    }

    private void jump()
    {
        if (!Jumpable) return;
        GameObject effect = Instantiate(jumpEffect, transform.position, Quaternion.identity);
        rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        Destroy(effect, 0.15f);
    }

    private void skill()
    {
        if (!Skillable) return;
        if (Dashing) return;
        if (!Attackable || !Skillable) return;

        int dir = transform.localScale.x < 0 ? -1 : 1;
        rigid.AddForce(Vector2.right * dir * skillDashPower, ForceMode2D.Impulse);
        animator.Skill();

        var effect = Instantiate(skillEffect, transform.position, Quaternion.identity);
        effect.transform.localScale = new Vector3(dir, 1f, 1f);
        effect.GetComponent<Rigidbody2D>().velocity = Vector2.right * dir * 10f;
        Destroy(effect, 2f);
    }

    private void dash()
    {
        if (!Dashable) return;
        if (!Attackable || Skilling) return;

        int dir = transform.localScale.x < 0 ? -1 : 1;
        rigid.AddForce(Vector2.right * dir * dashPower, ForceMode2D.Impulse);

        animator.Dash();
        dashEffect();
    }

    private void dashEffect()
    {
        StartCoroutine(effectCoroutine());
    }

    private IEnumerator effectCoroutine()
    {
        for(int i = 0; i < 6; i++)
        {
            GameObject n = new GameObject();
            SpriteRenderer s = n.AddComponent<SpriteRenderer>();
            s.sprite = GetComponent<SpriteRenderer>().sprite;
            s.material = GetComponent<SpriteRenderer>().material;
            s.sortingLayerName = "PlayerEffect";
            s.sortingOrder = (i - 1) * -1;
            s.color = Color.black;
            n.transform.position = transform.position;
            yield return new WaitForSeconds(0.2f / 6f);
            Destroy(n, 0.2f);
        }
    }

    private void checkGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, groundMask);

        bool isGrounded = hit.collider != null;

        if (isGrounded)
        {
            Jumpable = true;
        }
        else
        {
            Jumpable = false;
        }

            Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector2.down * rayDistance, rayColor);
    }
    public void StartSkillCooltime()
    {
        StartCoroutine(skillCool());
    }
    public void StartDashCooltime()
    {
        StartCoroutine(dashCoroutine());
    }
    private IEnumerator dashCoroutine()
    {
        while (dashDuration <= dashCooltime)
        {
            dashDuration += Time.deltaTime;
            yield return null;
        }

        dashDuration = 0f;
        Dashable = true;
        yield break;
    }
    private IEnumerator skillCool()
    {
        while(skillDuration <= skillCooltime)
        {
            skillDuration += Time.deltaTime;
            yield return null;
        }

        skillDuration = 0f;
        Skillable = true;
        yield break;
    }
}

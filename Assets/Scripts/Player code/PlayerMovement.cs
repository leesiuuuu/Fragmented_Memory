using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    PlayerHP playerHP;


    int jumpCount = 0;


    public float moveSpeed = 9f;
    public float jumpPower = 8f;
    public float dashPower = 13f;
    public float dashCoolTime = 0.2f;
    public float dashTime = 0.3f;


    bool canDash = true;
    bool isDash = false;

    bool isGround = false;



    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerHP = GetComponent<PlayerHP>();
    }



    void Update()
    {
        if (playerHP.IsDead)
            return;


        float h = Input.GetAxisRaw("Horizontal");


        if (h < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (h > 0)
        {
            spriteRenderer.flipX = false;
        }



        // 점프

        if ((Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.UpArrow))
            && jumpCount < 2)
        {

            rigid.linearVelocity =
                new Vector2(
                    rigid.linearVelocity.x,
                    0
                );


            rigid.AddForce(
                Vector2.up * jumpPower,
                ForceMode2D.Impulse
            );


            jumpCount++;

            isGround = false;
        }



        // 대쉬

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            canDash = false;
            isDash = true;

            float direction = spriteRenderer.flipX ? -1 : 1;

            rigid.linearVelocity =
                new Vector2(
                    direction * dashPower,
                    rigid.linearVelocity.y
                );

            Invoke(nameof(EndDash), dashCoolTime);
            Invoke(nameof(ResetDash), dashTime);
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



        float h = Input.GetAxisRaw("Horizontal");


        if(!isDash)
        {
            rigid.linearVelocity =
                new Vector2(
                    h * moveSpeed,
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
    }



    void ResetDash()
    {
        canDash = true;
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
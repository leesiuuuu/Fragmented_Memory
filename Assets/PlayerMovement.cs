using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;

    public float moveSpeed = 9f;
    public float jumpPower = 4f;
    public float dashPower = 15f;

    bool isGround = false;
    bool canDash = true;
    bool isDash = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if (h < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (h > 0)
        {
            spriteRenderer.flipX = false;
        }


        if ((Input.GetKeyDown(KeyCode.Space)|| Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow)) && isGround)
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }


        if ((Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift)) && canDash)
        {
            canDash = false;
            isDash = true;

            if (spriteRenderer.flipX)
            {
                rigid.linearVelocity =
                    new Vector2(-dashPower, 0);
            }
            else
            {
                rigid.linearVelocity =
                    new Vector2(dashPower, 0);
            }

            Invoke("EndDash", 0.2f);
            Invoke("ResetDash", 1f);
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if (!isDash)
        {
            rigid.linearVelocity =
                new Vector2(h * moveSpeed,
                            rigid.linearVelocity.y);
        }
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
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }
}
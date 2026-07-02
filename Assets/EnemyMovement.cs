using UnityEngine;

public class Enemy : MonoBehaviour
{

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Transform player;

    // Movement

    public float moveSpeed = 5f;
    public float jumpPower = 8f;

    bool isGround = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
            return;
        Flip();
    }

    void FixedUpdate()
    {
        if (player == null)
            return;
        State();
    }


    void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    float GetDistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }


    void Move()
    {
        if (player.position.x < transform.position.x)
        {
            rigid.linearVelocity =
                new Vector2(-1 * moveSpeed,
                            rigid.linearVelocity.y);
        }
        else if (player.position.x > transform.position.x)
        {
            rigid.linearVelocity =
                new Vector2(1 * moveSpeed,
                            rigid.linearVelocity.y);
        }
        if (player.position.y > transform.position.y + 1f && isGround == true && GetDistanceToPlayer() <3)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }
    }


    void State()
    {
        if (GetDistanceToPlayer() < 6)
        {
        Move();
        }   
    }
    void Flip()
    {
        
        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else if (player.position.x >= transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        
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
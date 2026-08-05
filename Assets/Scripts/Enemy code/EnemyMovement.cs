using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Transform player;
    EnemyAttack enemyAttack;
    private EnemyStats stats;


    // Movement

    public float moveSpeed = 5f;
    public float distance = 1.3f;
    public float area = 6f;


    // 나중에 점프형 적 추가 시 사용
    //public float jumpPower = 8f;

    //bool isGround = false;



    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyAttack = GetComponent<EnemyAttack>();
        stats = GetComponent<EnemyStats>();
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
        GameObject target =
            GameObject.FindGameObjectWithTag("Player");


        if (target != null)
        {
            player = target.transform;
        }
    }



    public float GetDistanceToPlayer()
    {
        if(player == null)
            return Mathf.Infinity;


        return Vector2.Distance(
            transform.position,
            player.position
        );
    }




    void Move()
    {
        if(player.position.x < transform.position.x)
        {
            rigid.linearVelocity =
                new Vector2(
                    -moveSpeed,
                    rigid.linearVelocity.y
                );
        }
        else if(player.position.x > transform.position.x)
        {
            rigid.linearVelocity =
                new Vector2(
                    moveSpeed,
                    rigid.linearVelocity.y
                );
        }



        // 나중에 점프형 적 추가 시 사용
        /*
        if (player.position.y > transform.position.y + 1f 
            && isGround == true 
            && GetDistanceToPlayer() < 3)
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
        }
        */
    }




    void State()
    {
        float distanceToPlayer =
            GetDistanceToPlayer();



        if(distanceToPlayer < distance)
        {
            rigid.linearVelocity =
                new Vector2(
                    0,
                    rigid.linearVelocity.y
                );
        }
        else if(distanceToPlayer < area)
        {
            Move();
        }
        else
        {
            rigid.linearVelocity =
                new Vector2(
                    0,
                    rigid.linearVelocity.y
                );
        }
    }




    void Flip()
    {
        if(player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else if(player.position.x >= transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
    }



    /*
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
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
    */
}
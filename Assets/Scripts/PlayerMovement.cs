using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpPower = 10f;

    private Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float move = 0f;

        if (Keyboard.current.aKey.isPressed)
            move = -1f;
        else if (Keyboard.current.dKey.isPressed)
            move = 1f;

        rigid.linearVelocity = new Vector2(
            move * moveSpeed,
            rigid.linearVelocity.y
        );

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rigid.linearVelocity = new Vector2(
                rigid.linearVelocity.x,
                jumpPower
            );
        }
    }
}

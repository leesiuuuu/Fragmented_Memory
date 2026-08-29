using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    private EnemyMovement movement;
    private EnemyAttack attack;
    private Rigidbody2D rigid;
    private float stunEndTime;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        attack = GetComponent<EnemyAttack>();
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Apply(float duration)
    {
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);

        if (movement != null)
            movement.enabled = false;

        if (attack != null)
            attack.SetStunned(true);

        if (rigid != null)
            rigid.linearVelocity = Vector2.zero;

        CancelInvoke(nameof(Release));
        Invoke(nameof(Release), Mathf.Max(0f, stunEndTime - Time.time));
    }

    private void Release()
    {
        if (Time.time < stunEndTime)
        {
            Invoke(nameof(Release), stunEndTime - Time.time);
            return;
        }

        if (movement != null)
            movement.enabled = true;

        if (attack != null)
            attack.SetStunned(false);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Release));
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private SpriteRenderer visual;

    private readonly HashSet<EnemyHP> hitEnemies = new HashSet<EnemyHP>();
    private PlayerStats stats;
    private PlayerHP playerHP;
    private PlayerSynergyManager synergyManager;
    private Vector2 startPosition;
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private float damageMultiplier;
    private bool ignoreDefense;
    private Vector3 visualBaseScale;

    private void Awake()
    {
        if (visual != null)
            visualBaseScale = visual.transform.localScale;
    }

    public void Initialize(
        PlayerStats playerStats,
        PlayerHP ownerHP,
        Vector2 moveDirection,
        float moveSpeed,
        float distance,
        float multiplier,
        bool ignoresDefense = false)
    {
        stats = playerStats;
        playerHP = ownerHP;
        synergyManager = ownerHP != null ? ownerHP.GetComponent<PlayerSynergyManager>() : null;
        direction = moveDirection.normalized;
        speed = moveSpeed;
        maxDistance = distance;
        damageMultiplier = multiplier;
        ignoreDefense = ignoresDefense;
        startPosition = transform.position;

        if (visual != null)
        {
            visual.flipX = direction.x < 0f;
            visual.transform.localScale = new Vector3(
                visualBaseScale.x / transform.localScale.x,
                visualBaseScale.y / transform.localScale.y,
                visualBaseScale.z);
        }

    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = rigid.position + direction * speed * Time.fixedDeltaTime;
        rigid.MovePosition(nextPosition);

        if ((nextPosition - startPosition).sqrMagnitude
            >= maxDistance * maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHP enemy = other.GetComponentInParent<EnemyHP>();

        if (enemy == null || !hitEnemies.Add(enemy))
            return;

        int damage = Mathf.RoundToInt(stats.GetAttackDamage() * damageMultiplier);
        int dealtDamage = enemy.TakeDamage(damage, ignoreDefense);
        synergyManager?.OnDamageDealt(enemy, dealtDamage);

        if (dealtDamage > 0 && stats.CurrentLifeSteal > 0f)
            playerHP.Heal(Mathf.RoundToInt(dealtDamage * stats.CurrentLifeSteal / 100f));
    }
}

using UnityEngine;

public class EnemyDeathExplosion : MonoBehaviour
{
    [SerializeField] private float radius = 2.5f;

    private EnemyStats stats;


    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }


    public void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            PlayerHP playerHP = hit.GetComponentInParent<PlayerHP>();

            if (playerHP != null)
            {
                playerHP.TakeDamage(stats.attack);
                return;
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

using UnityEngine;

public class TraumaFragment : MonoBehaviour
{
    public float maxDistance = 8f;

    private Vector3 startPosition;
    private BossControl boss;

    public void SetBoss(BossControl bossControl)
    {
        boss = bossControl;
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (boss != null && boss.IsDead())
        {
            Destroy(gameObject);
            return;
        }

        if (Vector2.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}

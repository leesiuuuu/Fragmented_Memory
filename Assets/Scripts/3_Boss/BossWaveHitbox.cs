using UnityEngine;

public class BossWaveHitbox : MonoBehaviour
{
    [SerializeField] private int damage;

    private bool hit;

    public void SetDamage(int value)
    {
        damage = value;
        hit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hit)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHP playerHP = other.GetComponentInParent<PlayerHP>();

        if (playerHP == null)
            return;

        playerHP.TakeDamage(damage);
        hit = true;
    }

    private void OnDisable()
    {
        hit = false;
    }
}
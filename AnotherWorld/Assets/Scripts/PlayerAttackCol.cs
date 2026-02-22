using UnityEngine;

public class PlayerAttackCol : MonoBehaviour
{
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject floatingText;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int baseAD = PlayerStatManager.instance.AD;

            bool isCritical = Random.Range(0f, 100f) < PlayerStatManager.instance.Critical;

            int damage = baseAD;
            if (isCritical)
            {
                damage = Mathf.RoundToInt(baseAD * (1f + PlayerStatManager.instance.CriticalPercentage / 100f));
                Debug.Log("Critical Hit! Damage: " + damage);
            }

            float rand = Random.Range(0.2f, 0.9f);

            GameObject textObj = Instantiate(floatingText, 
                collision.transform.position + Vector3.up * rand, 
                Quaternion.identity);

            FloatingText textScript = textObj.GetComponent<FloatingText>();
            int printDamage = Mathf.Max(0, damage - collision.GetComponent<Enemy>().Defence);
            if (textScript != null)
            {
                textScript.SetText(printDamage.ToString(), isCritical);
            }

            GameManager.Instance.DamageResult += printDamage;

            collision.GetComponent<Enemy>().Hitted(damage);

            var obj = Instantiate(hitEffect,
                collision.transform.position,
                Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
            Destroy(obj, 0.2f);

            PlayerStatManager.instance.HP += PlayerStatManager.instance.LifeSteal;
            PlayerStatManager.instance.HP = Mathf.Min(PlayerStatManager.instance.HP, PlayerStatManager.instance.MaxHP);
        }
    }
}
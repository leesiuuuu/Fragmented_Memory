using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillCol : MonoBehaviour
{
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject floatingText;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int baseAd = PlayerStatManager.instance.AD * 2;

            float rand = Random.Range(0.2f, 0.9f);

            GameObject textObj = Instantiate(floatingText,
                collision.transform.position + Vector3.up * rand,
                Quaternion.identity);

            FloatingText textScript = textObj.GetComponent<FloatingText>();
            int printDamage = Mathf.Max(0, baseAd - collision.GetComponent<Enemy>().Defence);
            if (textScript != null)
            {
                textScript.SetText(printDamage.ToString(), false);
            }

            GameManager.Instance.DamageResult += printDamage;

            collision.GetComponent<Enemy>().Hitted(baseAd);

            var obj = Instantiate(hitEffect,
                collision.transform.position,
                Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

            Destroy(obj, 0.2f);
        }
    }
}

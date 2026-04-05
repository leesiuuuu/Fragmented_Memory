using UnityEngine;

public class EnemyAttackCol : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private GameObject floatingText;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 컴포넌트를 가져옴
            PlayerController player = collision.GetComponent<PlayerController>();

            // 2. null 체크 (스크립트를 못 찾았을 경우 대비)
            if (player != null)
            {
                player.Hitted(enemy.AD);
                Debug.Log($"플레이어에게 {enemy.AD}의 데미지를 입혔습니다.");

                float rand = Random.Range(0.2f, 0.9f);

                GameObject textObj = Instantiate(floatingText,
                    collision.transform.position + Vector3.up * rand,
                    Quaternion.identity);

                FloatingText textScript = textObj.GetComponent<FloatingText>();
                if (textScript != null)
                {
                    int printDamage = Mathf.Max(0, enemy.AD - PlayerStatManager.instance.Defence);
                    textScript.SetTextPlayer(printDamage.ToString());
                }

            }
            else
            {
                Debug.LogError("Player 태그는 찾았지만, PlayerController 스크립트를 찾을 수 없습니다!");
            }
        }
    }
}

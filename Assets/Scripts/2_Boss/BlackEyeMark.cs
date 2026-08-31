using System.Collections;
using UnityEngine;

public class BlackEyeMark : MonoBehaviour
{
    public GameObject markPrefab;
    public GameObject wavePrefab;

    public float markTime = 0.7f;
    public float waitTime = 0.4f;

    private BossControl boss;
    private bool isAttacking;

    void Start()
    {
        boss = GetComponent<BossControl>();

        if (boss != null)
            boss.OnPatternSelected += Pattern;
    }

    void OnDestroy()
    {
        if (boss != null)
            boss.OnPatternSelected -= Pattern;
    }

    void Pattern(int patternID, float damage)
    {
        if (patternID == 4 && !isAttacking)
            StartCoroutine(Attack(damage));
    }

    IEnumerator Attack(float damage)
    {
        isAttacking = true;

        if (boss == null ||
            boss.Player == null ||
            markPrefab == null ||
            wavePrefab == null)
        {
            isAttacking = false;
            yield break;
        }

        boss.SetCanMove(false);

        for (int i = 0; i < 4; i++)
        {
            Vector3 targetPosition = boss.Player.position;

            GameObject mark = Instantiate(
                markPrefab,
                targetPosition,
                Quaternion.identity
            );

            yield return new WaitForSeconds(markTime);

            GameObject wave = Instantiate(
                wavePrefab,
                transform.position,
                Quaternion.identity
            );

            BlackEyeMarkWave projectile =
                wave.GetComponent<BlackEyeMarkWave>();

            if (projectile != null)
            {
                projectile.SetTarget(
                    targetPosition,
                    mark,
                    boss,
                    damage
                );
            }
            else
            {
                Destroy(wave);
                Destroy(mark);
            }

            yield return new WaitForSeconds(waitTime);
        }

        boss.SetCanMove(true);
        isAttacking = false;
    }
}

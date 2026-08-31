using UnityEngine;

public class BossPhaseController : MonoBehaviour
{
    private BossControl boss;
    private bool phase2;

    void Awake()
    {
        boss = GetComponent<BossControl>();
    }

    void Update()
    {
        if (boss == null)
            return;

        if (!phase2 &&
            boss.GetCurrentHealth() <= boss.maxHealth * 0.5f)
        {
            phase2 = true;

            boss.AddAttackPattern(4, 400f);
            boss.AddAttackPattern(5, 900f);
        }
    }
}
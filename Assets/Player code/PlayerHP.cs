using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    [SerializeField] private int maxHP = 1500;

    private int currentHP;
    private bool isDead;

    [SerializeField] private HPBar hpBar;


    private void Start()
    {
        currentHP = maxHP;

        hpBar.SetHP(currentHP, maxHP);
    }


    public void TakeDamage(int damage)
    {
        if (isDead)
            return;


        currentHP -= damage;

        currentHP = Mathf.Max(currentHP, 0);


        hpBar.SetHP(currentHP, maxHP);


        if (currentHP <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        if (isDead)
            return;


        isDead = true;

        Debug.Log("dead");


        // 나중에 추가할 부분
        // 플레이어 이동 정지
        // 사망 애니메이션
        // 게임오버 처리
        // 재시작
    }


    public int GetCurrentHP()
    {
        return currentHP;
    }


    public int GetMaxHP()
    {
        return maxHP;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager instance;
    public int MaxHP = 200;
    public int HP;
    public int AD = 20;
    public int Defence = 5;
    public int Critical = 0;
    public int CriticalPercentage = 100;
    public int LifeSteal = 0;

    public bool PoisonAttack = false;
    public bool Turret = false;

    public List<CardInstance> cardInstances = new List<CardInstance>();
    public GameObject playerCamera;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool isGameEnd = false;

    private void Update()
    {
        if(HP <= 0 && !isGameEnd)
        {
            StartCoroutine(GameEnd());
        }
    }

    private IEnumerator GameEnd()
    {
        isGameEnd = true;

        playerCamera.SetActive(true);
        GameObject player = FindAnyObjectByType<PlayerController>().gameObject;
        player.GetComponent<Collider2D>().enabled = false;
        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<PlayerController>().enabled = false;

        yield return new WaitForSeconds(0.3f);

        player.GetComponent<PlayerAnimator>().Death();
        GameManager.Instance.CardInstances = cardInstances;
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("ResultScene");
        yield break;
    }

    private void Start()
    {
        HP = MaxHP;
    }

    public void AddStat(CardInstance cardInstance)
    {
        cardInstances.Add(cardInstance);

        applyStat(cardInstance);
    }

    private void applyStat(CardInstance cardInstance)
    {
        MaxHP = Mathf.Max(0, MaxHP + cardInstance.HP);
        HP = MaxHP;
        AD = Mathf.Max(0, AD + cardInstance.AD);
        Defence = Mathf.Max(0, Defence + cardInstance.Defence);
        Critical = Mathf.Max(0, Critical + cardInstance.Critical);
        CriticalPercentage = Mathf.Clamp(CriticalPercentage + cardInstance.CriticalPercentage, 0, 100); // 0~100%
        LifeSteal = Mathf.Max(0, LifeSteal + cardInstance.LifeSteal);

        // 특수 능력 적용
        if (cardInstance.SpecialAbility != SpecialAbilityType.None)
        {
            switch (cardInstance.SpecialAbility)
            {
                case SpecialAbilityType.PoisonOnHit: PoisonAttack = true; break;
                case SpecialAbilityType.Turret: Turret = true; break;
            }
        }
    }
}

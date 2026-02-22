using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatRandomizer : MonoBehaviour
{
    [Header("랜덤 선택할 카드 SO들")]
    public StatData[] statSOs;

    [Header("랜덤 추출된 값 UI 표기")]
    public List<StatCardUI> statCards;


    private void OnEnable()
    {
        FindObjectOfType<PlayerController>().enabled = false;
        if (statSOs == null || statSOs.Length == 0)
        {
            Debug.LogWarning("StatData SO 배열이 비어있습니다!");
            return;
        }
        for (int i = 0; i < statCards.Count; i++)
        {
            // 1. SO 배열에서 무작위 선택
            int index = Random.Range(0, statSOs.Length);
            StatData selectedSO = statSOs[index];

            statCards[i].SetCard(new CardInstance(selectedSO), selectedSO);
        }
    }
    private void OnDisable()
    {
        FindObjectOfType<PlayerController>().enabled = true;
    }
}

public class CardInstance
{
    public string Name;
    public int HP;
    public int AD;
    public int Defence;
    public int Critical;
    public int CriticalPercentage;
    public int LifeSteal;
    public string Description;
    public SpecialAbilityType SpecialAbility;

    public CardInstance(StatData data)
    {
        Name = data.Name;
        Description = data.Description;

        // 능력 결정
        if (data.SpecialAbility == SpecialAbilityType.Random)
        {
            GetRandomValue(data, 20);
            return;
        }
        else if(data.SpecialAbility == SpecialAbilityType.RandomPlus)
        {
            GetRandomValue(data, 40);
            return;
        }
        else
        {
            SpecialAbility = data.SpecialAbility;

            // SO에 적힌 스탯 그대로 사용
            HP = data.HP;
            AD = data.AD;
            Defence = data.Defence;
            Critical = data.Critical;
            CriticalPercentage = data.CriticalPercentage;
            LifeSteal = data.LifeSteal;
        }

    }

    private void GetRandomValue(StatData data, int percent)
    {
        HP = Random.Range(-percent, percent + 1);
        AD = Random.Range(-percent, percent + 1);
        Defence = Random.Range(-percent, percent + 1);
        Critical = Random.Range(-percent, percent + 1);
        CriticalPercentage = Random.Range(-percent, percent + 1);
        LifeSteal = Random.Range(-percent, percent + 1);
    }

    public override string ToString()
    {
        return $"Card: {Name}, HP:{HP}, AD:{AD}, DEF:{Defence}, CRIT:{Critical}({CriticalPercentage}%), LS:{LifeSteal}, Ability:{SpecialAbility}";
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StatCardUI : MonoBehaviour
{
    [SerializeField] private GameObject Canvas;
    [Header("UI Text Components")]
    public Text TitleText;
    public Text statsText; // 변동 스탯을 모두 하나의 Text에 표시
    public Image Icon;

    private StatData baseSO;
    private CardInstance cardInstance;

    // 카드 표시
    public void SetCard(CardInstance instance, StatData so)
    {
        cardInstance = instance;
        Debug.Log(instance.ToString());
        baseSO = so;
        TitleText.text = instance.Name;
        Icon.sprite = so.Icon;

        // 변동 스탯만 표시
        statsText.text = GetChangedStatsText();
    }

    public void SelectStat()
    {
        StartCoroutine(selectStat());
    }

    private IEnumerator selectStat()
    {
        Debug.Log("추가됨");
        PlayerStatManager.instance.AddStat(cardInstance);

        yield return new WaitForSeconds(0.4f);
        Canvas.SetActive(false);
    }

    private string GetChangedStatsText()
    {
        string result = "";

        if(cardInstance.Description != string.Empty)
        {
            result = cardInstance.Description;
            return result;
        }

        AppendIfChanged(ref result, "HP", cardInstance.HP);
        AppendIfChanged(ref result, "AD", cardInstance.AD);
        AppendIfChanged(ref result, "Defence", cardInstance.Defence);
        AppendIfChanged(ref result, "Critical", cardInstance.Critical);
        AppendIfChanged(ref result, "Crit %", cardInstance.CriticalPercentage);
        AppendIfChanged(ref result, "LifeSteal", cardInstance.LifeSteal);

        return result;
    }

    private string NameToKorean(string value)
    {
        switch (value)
        {
            case "HP": return "체력";
            case "AD": return "공격력";
            case "Defence": return "방어력";
            case "Critical": return "치명타 확률";
            case "Crit %": return "치명타 데미지";
            case "LifeSteal": return "기본 공격 피해 흡혈";
        }

        return null;
    }
    private void AppendIfChanged(ref string result, string statName, int currentValue)
    {
        int diff = currentValue > 0 ? currentValue : 0;
        if (diff != 0)
        {
            string sign = diff > 0 ? "+" : "";
            string statNameKorean = NameToKorean(statName);
            result += $"{statNameKorean} {sign}{diff}\n";
        }
    }
}
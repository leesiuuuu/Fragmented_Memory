using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlotUI : MonoBehaviour
{
    private TMP_Text nameText;
    private TMP_Text descriptionText;
    private TMP_Text buttonText;
    private Image icon;
    private Button selectButton;

    private RewardUI rewardUI;
    private int rewardIndex;
    private bool isBound;

    private void Awake()
    {
        BindComponents();
    }

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(Select);
    }

    public void Setup(RewardUI owner, int index, MemoryData memory)
    {
        BindComponents();

        rewardUI = owner;
        rewardIndex = index;

        if (nameText != null)
            nameText.text = $"[{GetRarityName(memory.rarity)}] {memory.memoryName}";

        if (descriptionText != null)
            descriptionText.text = BuildDescription(memory);

        if (buttonText != null)
            buttonText.text = "선택";

        if (icon != null)
        {
            icon.sprite = memory.icon;
            icon.enabled = memory.icon != null;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void BindComponents()
    {
        if (isBound)
            return;

        nameText = FindChild("NameText")?.GetComponent<TMP_Text>();
        descriptionText = FindChild("DescriptionText")?.GetComponent<TMP_Text>();
        buttonText = FindChild("Text (TMP)")?.GetComponent<TMP_Text>();
        icon = FindChild("Icon")?.GetComponent<Image>();
        selectButton = FindChild("SelectButton")?.GetComponent<Button>();

        if (selectButton != null)
            selectButton.onClick.AddListener(Select);

        isBound = true;
    }

    private Transform FindChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private void Select()
    {
        rewardUI?.SelectReward(rewardIndex);
    }

    private string BuildDescription(MemoryData memory)
    {
        StringBuilder builder = new StringBuilder(memory.description);

        AppendStat(builder, "체력", memory.health);
        AppendStat(builder, "공격력", memory.attack);
        AppendStat(builder, "방어력", memory.defense);
        AppendStat(builder, "치명타 확률", memory.criticalChance, "%");
        AppendStat(builder, "치명타 피해", memory.criticalDamage, "%");
        AppendStat(builder, "피흡", memory.lifeSteal, "%");
        AppendStat(builder, "매력", memory.charm);

        return builder.ToString();
    }

    private void AppendStat(StringBuilder builder, string label, float value, string suffix = "")
    {
        if (Mathf.Approximately(value, 0f))
            return;

        builder.AppendLine();
        builder.Append(label);
        builder.Append(value > 0f ? " +" : " ");
        builder.Append(value.ToString("0.#"));
        builder.Append(suffix);
    }

    private string GetRarityName(MemoryRarity rarity)
    {
        switch (rarity)
        {
            case MemoryRarity.Common:
                return "흔함";
            case MemoryRarity.Rare:
                return "희귀함";
            case MemoryRarity.Important:
                return "중요함";
            case MemoryRarity.Legendary:
                return "전설";
            default:
                return rarity.ToString();
        }
    }
}

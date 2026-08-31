using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스킬 5칸 쿨타임 바를 런타임에 만든다. 칸 프리팹이 따로 없어도 캔버스만 지정하면 동작한다.
// 표시할 키와 남은 시간은 SkillUi가 SkillManager에서 직접 읽는다.
public class SkillCooldownUI : MonoBehaviour
{
    private const int SkillSlotCount = 5;

    private static readonly SkillSlot[] SlotOrder =
    {
        SkillSlot.Slash,
        SkillSlot.StrongStrike,
        SkillSlot.Poke,
        SkillSlot.Strike,
        SkillSlot.Ultimate
    };

    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private Vector2 slotSize = new Vector2(72f, 72f);
    [SerializeField] private float slotSpacing = 12f;
    [SerializeField] private Vector2 anchoredPosition = new Vector2(0f, 40f);
    [SerializeField] private Color slotColor = new Color(0.12f, 0.12f, 0.18f, 0.85f);

    // SkillSlot 순서대로 넣는다. 비워 두면 그 칸은 slotColor 단색으로 남는다.
    [SerializeField] private Sprite[] skillIcons = new Sprite[SkillSlotCount];

    private readonly List<SkillUi> slots = new List<SkillUi>();

    private void Awake()
    {
        if (skillManager == null)
            skillManager = GetComponent<SkillManager>();

        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();

        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>();

        if (skillManager == null || targetCanvas == null)
        {
            Debug.LogError("[SkillCooldownUI] SkillManager 또는 Canvas를 찾지 못했습니다.", this);
            return;
        }

        Build();
    }

    private void Build()
    {
        GameObject barObject = new GameObject("SkillCooldownBar");
        barObject.transform.SetParent(targetCanvas.transform, false);

        RectTransform bar = barObject.AddComponent<RectTransform>();
        bar.anchorMin = new Vector2(0.5f, 0f);
        bar.anchorMax = new Vector2(0.5f, 0f);
        bar.pivot = new Vector2(0.5f, 0f);
        bar.anchoredPosition = anchoredPosition;
        bar.sizeDelta = new Vector2(
            SlotOrder.Length * slotSize.x + (SlotOrder.Length - 1) * slotSpacing,
            slotSize.y);

        HorizontalLayoutGroup layout = barObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = slotSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        foreach (SkillSlot slot in SlotOrder)
            slots.Add(CreateSlot(bar, slot));
    }

    private SkillUi CreateSlot(RectTransform parent, SkillSlot slot)
    {
        GameObject slotObject = new GameObject($"Skill_{slot}");
        slotObject.transform.SetParent(parent, false);

        RectTransform rect = slotObject.AddComponent<RectTransform>();
        rect.sizeDelta = slotSize;

        Sprite icon = GetIcon(slot);

        Image background = slotObject.AddComponent<Image>();
        background.sprite = icon;
        // 아이콘에 이미 배경 타일이 그려져 있으므로 원색 그대로 띄운다.
        background.color = icon != null ? Color.white : slotColor;
        background.raycastTarget = false;

        TMP_Text keyText = CreateText(rect, "Key", 22f,
            TextAlignmentOptions.TopLeft, new Vector4(8f, 4f, 4f, 4f));
        TMP_Text cooldownText = CreateText(rect, "Cooldown", 34f,
            TextAlignmentOptions.Center, Vector4.zero);

        SkillUi ui = slotObject.AddComponent<SkillUi>();
        ui.Build(skillManager, slot, background, keyText, cooldownText);

        return ui;
    }

    private Sprite GetIcon(SkillSlot slot)
    {
        int index = (int)slot;

        if (skillIcons == null || index < 0 || index >= skillIcons.Length)
            return null;

        return skillIcons[index];
    }

    private static TMP_Text CreateText(RectTransform parent, string name, float fontSize,
        TextAlignmentOptions alignment, Vector4 margin)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.margin = margin;
        text.color = Color.white;
        text.raycastTarget = false;

        return text;
    }
}

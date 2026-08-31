using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스킬 한 칸의 표시를 맡는다. 남은 쿨타임은 SkillManager가 실제로 돌리는 값을 그대로 읽으므로
// 상점의 스킬 가속제처럼 쿨타임을 줄이는 효과도 그대로 반영된다.
public class SkillUi : MonoBehaviour
{
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private SkillSlot slot;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private Image skillImage;
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color cooldownColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private bool onCooldown;

    // 쿨타임 중 아이콘을 어둡게 깔아 두는 비율.
    private const float CooldownDim = 0.4f;

    // SkillCooldownUI가 런타임으로 칸을 만들 때 참조를 한 번에 넘긴다.
    public void Build(SkillManager manager, SkillSlot targetSlot,
        Image image, TMP_Text key, TMP_Text cooldown)
    {
        skillManager = manager;
        slot = targetSlot;
        skillImage = image;
        keyText = key;
        cooldownText = cooldown;

        // 칸을 만든 쪽이 정한 색(아이콘이면 흰색, 아니면 단색 배경)을 준비 상태 색으로 삼는다.
        if (skillImage != null)
        {
            readyColor = skillImage.color;
            cooldownColor = new Color(
                readyColor.r * CooldownDim,
                readyColor.g * CooldownDim,
                readyColor.b * CooldownDim,
                readyColor.a);
        }

        RefreshKeyLabel();
        SetCooldownVisible(false);
    }

    private void Awake()
    {
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();
    }

    private void Start()
    {
        RefreshKeyLabel();
        SetCooldownVisible(false);
    }

    private void Update()
    {
        if (skillManager == null)
            return;

        float remaining = skillManager.GetCooldownRemaining(slot);

        if (remaining <= 0f)
        {
            if (onCooldown)
                SetCooldownVisible(false);

            return;
        }

        if (!onCooldown)
            SetCooldownVisible(true);

        if (cooldownText != null)
            cooldownText.text = Mathf.Ceil(remaining).ToString();
    }

    private void SetCooldownVisible(bool visible)
    {
        onCooldown = visible;

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(visible);

            if (!visible)
                cooldownText.text = string.Empty;
        }

        if (skillImage != null)
            skillImage.color = visible ? cooldownColor : readyColor;
    }

    private void RefreshKeyLabel()
    {
        if (keyText == null || skillManager == null)
            return;

        keyText.text = KeyLabel(skillManager.GetSkillKey(slot));
    }

    // KeyCode를 그대로 찍으면 "Alpha1"처럼 나오므로 화면에 쓸 이름으로 줄인다.
    private static string KeyLabel(KeyCode key)
    {
        if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
            return ((int)(key - KeyCode.Alpha0)).ToString();

        switch (key)
        {
            case KeyCode.None: return string.Empty;
            case KeyCode.Mouse0: return "L-Click";
            case KeyCode.Mouse1: return "R-Click";
            case KeyCode.LeftShift: return "L-Shift";
            case KeyCode.RightShift: return "R-Shift";
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
        }

        return key.ToString();
    }
}

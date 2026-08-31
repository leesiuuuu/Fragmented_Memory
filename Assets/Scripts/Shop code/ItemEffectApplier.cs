using UnityEngine;

public class ItemEffectApplier : MonoBehaviour
{
    private PlayerStats playerStats;
    private PlayerHP playerHP;
    private PlayerAttack playerAttack;
    private PlayerMovement playerMovement;
    private SkillManager skillManager;

    public void Initialize(GameObject player)
    {
        if (player == null)
            return;

        playerStats = player.GetComponent<PlayerStats>();
        playerHP = player.GetComponent<PlayerHP>();
        playerAttack = player.GetComponent<PlayerAttack>();
        playerMovement = player.GetComponent<PlayerMovement>();
        skillManager = player.GetComponent<SkillManager>();
    }

    public bool CanApply(ItemData item)
    {
        if (item == null)
            return false;

        switch (item.effectType)
        {
            case ItemEffectType.Heal:
                return playerHP != null;
            case ItemEffectType.Attack:
            case ItemEffectType.Defense:
            case ItemEffectType.CriticalChance:
                return playerStats != null;
            case ItemEffectType.AttackCooldown:
                return playerAttack != null;
            case ItemEffectType.JumpCount:
            case ItemEffectType.DashCount:
                return playerMovement != null;
            case ItemEffectType.SkillCooldown:
                return skillManager != null;
            case ItemEffectType.Revival:
                return false;
        }

        return false;
    }

    public void Apply(ItemData item)
    {
        if (!CanApply(item))
            return;

        if (item.effectType == ItemEffectType.Heal)
        {
            playerHP.Heal(Mathf.RoundToInt(item.effectValue));
            return;
        }

        if (item.effectType == ItemEffectType.AttackCooldown)
        {
            playerAttack.ApplyAttackCooldownReduction(item.effectValue, item.duration);
            return;
        }

        if (item.effectType == ItemEffectType.JumpCount)
        {
            playerMovement.AddMaxJumpCount(Mathf.RoundToInt(item.effectValue));
            return;
        }

        if (item.effectType == ItemEffectType.DashCount)
        {
            playerMovement.AddMaxDashCount(Mathf.RoundToInt(item.effectValue));
            return;
        }

        if (item.effectType == ItemEffectType.SkillCooldown)
        {
            skillManager.ApplySkillCooldownReduction(item.effectValue, item.duration);
            return;
        }

        if (item.effectType == ItemEffectType.CriticalChance && item.duration > 0f)
        {
            playerStats.ApplyTemporaryCriticalChance(item.effectValue, item.duration);
            return;
        }

        StatData stat = new StatData();

        switch (item.effectType)
        {
            case ItemEffectType.Attack:
                stat.attack = Mathf.RoundToInt(item.effectValue);
                break;
            case ItemEffectType.Defense:
                stat.defense = Mathf.RoundToInt(item.effectValue);
                break;
            case ItemEffectType.CriticalChance:
                stat.criticalChance = item.effectValue;
                break;
        }

        playerStats.ApplyStat(stat);
    }
}

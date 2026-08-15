using UnityEngine;

public static class DamageCalculator
{
    private const float DefenseConstant = 500f;

    public static int Calculate(float rawDamage, float defense, bool ignoreDefense = false)
    {
        rawDamage = Mathf.Max(0f, rawDamage);
        if (ignoreDefense) return Mathf.Max(1, Mathf.RoundToInt(rawDamage));

        defense = Mathf.Max(0f, defense);
        float reduction = defense / (defense + DefenseConstant);
        float finalDamage = rawDamage * (1f - reduction);
        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }
}

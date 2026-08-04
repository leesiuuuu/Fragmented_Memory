using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Transform fill;

    public void SetHP(int currentHP, int maxHP)
{
    float ratio = (float)currentHP / maxHP;
    ratio = Mathf.Clamp01(ratio);

    fill.localScale = new Vector3(ratio, 1f, 1f);
    fill.localPosition = new Vector3(-(1 - ratio) * 0.5f, 0f, 0f);
}
}
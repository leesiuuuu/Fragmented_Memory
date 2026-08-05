using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Transform fill;

    public void SetHP(int currentHP, int maxHP)
    {
        if (fill == null)
        {
            Debug.LogError("HPBar Fill is not assigned");
            return;
        }

        if (maxHP <= 0)
        {
            Debug.LogError("HPBar maxHP is 0 or less");
            return;
        }


        float ratio = (float)currentHP / maxHP;

        ratio = Mathf.Clamp01(ratio);


        fill.localScale = new Vector3(
            ratio,
            1f,
            1f
        );


        fill.localPosition = new Vector3(
            -(1f - ratio) * 0.5f,
            0f,
            0f
        );
    }
}
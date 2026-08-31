using UnityEngine;

public class BossHealthBarPosition : MonoBehaviour
{
    public float topDistance = 50f;

    void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);

        rect.pivot = new Vector2(0.5f, 1f);

        rect.anchoredPosition = new Vector2(
            0f,
            -topDistance
        );
    }
}
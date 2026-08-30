using UnityEngine;

public class BossHealthBarPosition : MonoBehaviour
{
    public float topDistance = 50f;

    void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        RectTransform rect = GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);

        rect.pivot = new Vector2(0.5f, 1f);

        rect.anchoredPosition = new Vector2(
            0f,
            -topDistance
        );
    }
}
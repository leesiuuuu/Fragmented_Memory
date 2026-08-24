using UnityEngine;

public class SkillAreaVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;

    public void Show(Vector2 size, Color color, int sortingOrder, float duration)
    {
        transform.localScale = new Vector3(size.x, size.y, 1f);

        if (visual != null)
        {
            visual.color = color;
            visual.sortingOrder = sortingOrder;
        }

        Destroy(gameObject, duration);
    }
}

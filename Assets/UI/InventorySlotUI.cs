using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    private Sprite placeholderSprite;

    private void Awake()
    {
        if (icon != null)
            placeholderSprite = icon.sprite;
    }

    public void Show(MemoryData memory)
    {
        if (icon == null)
            return;

        if (memory == null)
        {
            icon.enabled = false;
            return;
        }

        icon.sprite = memory.icon != null ? memory.icon : placeholderSprite;
        icon.enabled = true;
    }
}

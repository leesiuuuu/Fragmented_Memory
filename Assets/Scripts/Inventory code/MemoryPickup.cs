using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class MemoryPickup : MonoBehaviour, InteractRule
{
    private MemoryData memory;
    private Inventory nearbyInventory;

    public void Initialize(MemoryData data)
    {
        memory = data;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (memory != null && memory.icon != null)
            spriteRenderer.sprite = memory.icon;
    }

    public void Interact()
    {
        if (nearbyInventory != null && nearbyInventory.AddMemory(memory))
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            nearbyInventory = other.GetComponent<Inventory>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            nearbyInventory = null;
    }
}

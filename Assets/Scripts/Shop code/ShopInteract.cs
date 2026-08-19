using UnityEngine;

public class ShopInteract : MonoBehaviour, InteractRule
{
    [SerializeField] private ShopUI shopUI;

    private void Awake()
    {
        shopUI?.Initialize(GetComponent<ShopManager>());
    }

    public void Interact()
    {
        shopUI?.Open();
    }

    public void SetAvailable(bool available)
    {
        if (!available)
            shopUI?.Close();

        gameObject.SetActive(available);
    }
}

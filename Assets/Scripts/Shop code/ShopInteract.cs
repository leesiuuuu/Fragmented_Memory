using UnityEngine;

public class ShopInteract : MonoBehaviour, InteractRule
{
    [SerializeField] private ShopUI shopUI;

    private void Awake()
    {
        EnsureShopUI();
        shopUI?.Initialize(GetComponent<ShopManager>());
    }

    public void Interact()
    {
        // 방 프리팹 안의 상점은 씬의 ShopUI를 직렬화할 수 없다.
        // 열기 직전에 자기 ShopManager를 다시 물려야 현실 상점과 미러 상점이 같은 UI를 쓴다.
        EnsureShopUI();
        shopUI?.Initialize(GetComponent<ShopManager>());
        shopUI?.Open();
    }

    private void EnsureShopUI()
    {
        if (shopUI == null)
            shopUI = FindFirstObjectByType<ShopUI>(FindObjectsInactive.Include);
    }

    public void SetAvailable(bool available)
    {
        if (!available)
            shopUI?.Close();

        gameObject.SetActive(available);
    }
}

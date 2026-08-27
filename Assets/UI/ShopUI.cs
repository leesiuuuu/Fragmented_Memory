using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상점 목록을 표시한다. 선택한 상품의 정보와 구매 입력은 ShopManager에 넘긴다.
// 상점이 열려 있는 동안에는 플레이 입력을 막고 E를 다시 누르면 닫는다.
public class ShopUI : MonoBehaviour
{
    private const string InputLockId = "shop";
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private KeyCode closeKey = KeyCode.E;
    [SerializeField] private TMP_Text starDustText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailPriceText;
    [SerializeField] private Button detailBuyButton;
    [SerializeField] private List<ShopItemSlot> slots = new List<ShopItemSlot>();

    private CurrencyWallet wallet;
    private int selectedIndex = -1;
    private int openedFrame;

    public void Initialize(ShopManager manager)
    {
        shopManager = manager;
    }

    private void Awake()
    {
        if (detailBuyButton != null)
            detailBuyButton.onClick.AddListener(BuySelected);
    }

    private void Update()
    {
        if (Time.frameCount > openedFrame && Input.GetKeyDown(closeKey))
            Close();
    }

    private void OnEnable()
    {
        if (shopManager != null)
            shopManager.ShopChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (shopManager != null)
            shopManager.ShopChanged -= Refresh;
        if (wallet != null)
            wallet.StarDustChanged -= UpdateStarDust;
    }

    private void OnDestroy()
    {
        if (detailBuyButton != null)
            detailBuyButton.onClick.RemoveListener(BuySelected);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        openedFrame = Time.frameCount;
        GameplayInputLock.SetLocked(InputLockId, true);
        selectedIndex = -1;
        Refresh();
    }

    public void Close()
    {
        GameplayInputLock.SetLocked(InputLockId, false);
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (shopManager == null)
            return;

        BindWallet();
        EnsureSlotCount(shopManager.CurrentItems.Count);

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < shopManager.CurrentItems.Count)
            {
                slots[i].Setup(i, shopManager.CurrentItems[i], SelectItem);
                slots[i].SetSelected(i == selectedIndex);
            }
            else
            {
                slots[i].Hide();
            }
        }

        RefreshDetails();
    }

    private void EnsureSlotCount(int count)
    {
        if (slots.Count == 0 || slots[0] == null)
            return;

        while (slots.Count < count)
            slots.Add(Instantiate(slots[0], slots[0].transform.parent));

    }

    private void SelectItem(int index)
    {
        if (index < 0 || index >= shopManager.CurrentItems.Count)
            return;

        selectedIndex = index;
        Refresh();
    }

    private void BuySelected()
    {
        if (selectedIndex < 0)
            return;

        PurchaseResult result = shopManager.Buy(selectedIndex);

        if (result == PurchaseResult.Success && detailDescriptionText != null)
        {
            ItemData item = shopManager.CurrentItems[selectedIndex];
            detailDescriptionText.text = $"{item.description}\n\n구매 완료\n\n<size=70%><color=#FFFFFF>E 닫기</color></size>";
        }
    }

    private void RefreshDetails()
    {
        bool hasSelection = selectedIndex >= 0
            && selectedIndex < shopManager.CurrentItems.Count;

        if (!hasSelection)
        {
            if (detailIcon != null)
            {
                detailIcon.sprite = null;
                detailIcon.enabled = false;
            }
            if (detailNameText != null)
                detailNameText.text = "상품을 선택하세요";
            if (detailDescriptionText != null)
                detailDescriptionText.text = "<size=70%><color=#FFFFFF>E 닫기</color></size>";
            if (detailPriceText != null)
                detailPriceText.text = string.Empty;
            if (detailBuyButton != null)
                detailBuyButton.interactable = false;
            return;
        }

        ItemData item = shopManager.CurrentItems[selectedIndex];
        if (detailIcon != null)
        {
            detailIcon.sprite = item.icon;
            detailIcon.enabled = item.icon != null;
        }
        if (detailNameText != null)
            detailNameText.text = item.itemName;
        if (detailDescriptionText != null)
            detailDescriptionText.text = $"{item.description}\n\n<size=70%><color=#FFFFFF>E 닫기</color></size>";
        if (detailPriceText != null)
            detailPriceText.text = $"가격  {item.price}";
        if (detailBuyButton != null)
            detailBuyButton.interactable = true;
    }

    private void BindWallet()
    {
        if (wallet == shopManager.Wallet)
            return;

        if (wallet != null)
            wallet.StarDustChanged -= UpdateStarDust;

        wallet = shopManager.Wallet;

        if (wallet != null)
        {
            wallet.StarDustChanged += UpdateStarDust;
            UpdateStarDust(wallet.StarDust);
        }
    }

    private void UpdateStarDust(int amount)
    {
        if (starDustText != null)
            starDustText.text = amount.ToString();
    }
}

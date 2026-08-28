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
    private TMP_Text purchaseResultText;
    private GameObject purchaseResultBackground;

    public void Initialize(ShopManager manager)
    {
        shopManager = manager;
    }

    private void Awake()
    {
        if (detailBuyButton != null)
            detailBuyButton.onClick.AddListener(BuySelected);

        if (detailDescriptionText != null)
        {
            GameObject popupObject = Instantiate(
                detailDescriptionText.gameObject,
                transform);
            popupObject.name = "PurchaseResultPopup";
            purchaseResultText = popupObject.GetComponent<TMP_Text>();

            RectTransform popupRect = popupObject.transform as RectTransform;
            if (popupRect != null)
            {
                popupRect.anchorMin = new Vector2(0.5f, 0.5f);
                popupRect.anchorMax = new Vector2(0.5f, 0.5f);
                popupRect.anchoredPosition = Vector2.zero;
                popupRect.sizeDelta = new Vector2(600f, 180f);
            }

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(transform, false);
            purchaseResultBackground = backgroundObject;

            RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(600f, 180f);

            RawImage background = backgroundObject.AddComponent<RawImage>();
            if (background != null)
            {
                background.texture = Texture2D.whiteTexture;
                background.color = new Color(0f, 0f, 0f, 0.55f);
                background.raycastTarget = false;
            }
            backgroundObject.transform.SetSiblingIndex(popupObject.transform.GetSiblingIndex());
            if (purchaseResultText != null)
            {
                purchaseResultText.alignment = TextAlignmentOptions.Center;
                purchaseResultText.fontSize = 40f;
                purchaseResultText.fontWeight = FontWeight.Bold;
                purchaseResultText.transform.SetAsLastSibling();
            }
            popupObject.SetActive(false);
            backgroundObject.SetActive(false);
        }
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
        selectedIndex = shopManager.CurrentItems.Count > 0 ? 0 : -1;
        Refresh();
    }

    public void Close()
    {
        HidePurchaseResult();
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

        ShowPurchaseResult(result == PurchaseResult.Success ? "구매 완료" : "구매 실패");
    }

    private void ShowPurchaseResult(string message)
    {
        if (purchaseResultText == null)
            return;

        purchaseResultText.text = message;
        purchaseResultText.transform.SetAsLastSibling();
        purchaseResultText.gameObject.SetActive(true);
        if (purchaseResultBackground != null)
            purchaseResultBackground.SetActive(true);
        CancelInvoke(nameof(HidePurchaseResult));
        Invoke(nameof(HidePurchaseResult), 1f);
    }

    private void HidePurchaseResult()
    {
        CancelInvoke(nameof(HidePurchaseResult));

        if (purchaseResultText != null)
            purchaseResultText.gameObject.SetActive(false);
        if (purchaseResultBackground != null)
            purchaseResultBackground.SetActive(false);
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
                detailDescriptionText.text = string.Empty;
            if (detailPriceText != null)
                detailPriceText.text = string.Empty;
            if (detailBuyButton != null)
                detailBuyButton.interactable = false;
            return;
        }

        ItemData item = shopManager.CurrentItems[selectedIndex];
        if (detailIcon != null)
        {
            detailIcon.sprite = item.detailIcon;
            detailIcon.enabled = item.detailIcon != null;
        }
        if (detailNameText != null)
            detailNameText.text = item.itemName;
        if (detailDescriptionText != null)
            detailDescriptionText.text = item.description;
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
            starDustText.text = $"별의 가루: {amount}";
    }
}

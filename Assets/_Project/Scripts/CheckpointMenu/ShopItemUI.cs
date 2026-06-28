using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;

    private ShopItemSO currentItem;
    private Checkpoint checkpointManager;

    public void Initialize(ShopItemSO item, Checkpoint manager)
    {
        currentItem = item;
        checkpointManager = manager;

        if (iconImage) iconImage.sprite = item.Icon;
        if (nameText) nameText.text = item.ItemName;
        if (descText) descText.text = item.Description;
        if (priceText) priceText.text = $"{item.Price} эхо";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => checkpointManager.TryBuyItem(currentItem, this));
    }

    public void UpdateAvailability(int playerEcho)
    {
        bool canAfford = playerEcho >= currentItem.Price;
        buyButton.interactable = canAfford;

        var btnText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.text = canAfford ? "КУПИТЬ" : "НЕТ ЭХО";
    }

    public void SetButtonPurchased()
    {
        buyButton.interactable = false;

        var btnText = buyButton.GetComponentInChildren <TextMeshProUGUI>();
        if(btnText != null)
        {
            btnText.text = "КУПЛЕНО";
            btnText.color = Color.green;
        }
    }
}

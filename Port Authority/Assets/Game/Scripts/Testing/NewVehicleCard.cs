using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewVehicleCard : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text upgradeTitleText;
    public TMP_Text priceText;
    public TMP_Text effectText;
    public Button currentButton;

    [Header("Prefabs")]
    public Button buyButtonPrefab;
    public Button boughtButtonPrefab;

    private bool isPurchased;
    private System.Action onBuyCallback;

    public void SetUpgradeInfo(string title, string price, string effect, System.Action onBuy, bool isPurchased = false)
    {
        upgradeTitleText.text = title;
        priceText.text = price;
        effectText.text = effect;

        this.onBuyCallback = onBuy;
        this.isPurchased = isPurchased;

        currentButton.transform.SetAsLastSibling();

        TMP_Text label = currentButton.GetComponentInChildren<TMP_Text>();

        if (isPurchased)
        {
            if (boughtButtonPrefab != null)
            {
                currentButton.image.sprite = boughtButtonPrefab.image.sprite;
                currentButton.image.color = boughtButtonPrefab.image.color;
            }

            currentButton.interactable = false;
            if (label != null) label.text = "Owned";
            currentButton.onClick.RemoveAllListeners();
        }
        else
        {
            if (buyButtonPrefab != null)
            {
                currentButton.image.sprite = buyButtonPrefab.image.sprite;
                currentButton.image.color = buyButtonPrefab.image.color;
            }

            if (label != null)
            {
                label.text = "Buy";
            }

            currentButton.onClick.RemoveAllListeners();
            currentButton.onClick.AddListener(() => onBuyCallback?.Invoke());
        }
    }
}
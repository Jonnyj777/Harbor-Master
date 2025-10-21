using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreScript : MonoBehaviour
{
    [Header("UI")]
    public GameObject storePanel;
    public TextMeshProUGUI repairSpeedText;

    [Header("Cost Multiplier")]
    public float costMultiplier = 1.75f;    // Price increase each upgrade

    [Header("Repair Speed Upgrade Settings")]
    public int baseRepairSpeedCost = 150;
    public float repairSpeedCostMult = 0.1f;  // Each upgrade reduces base cost by 10% of original
    public int maxRepairSpeedLevel = 5;

    private int currentRepairSpeedLevel = 0;
    private int currentRepairSpeedCost;

    private void Start()
    {
        currentRepairSpeedCost = baseRepairSpeedCost;

        UpdateRepairSpeedEntry();
    }

    public void OpenStore()
    {
        storePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseStore()
    {
        storePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PurchaseRepairSpeedUpgrade()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= currentRepairSpeedCost
                && currentRepairSpeedLevel <= maxRepairSpeedLevel)
        {
            ScoreManager.Instance.UpdateSpendableScore(-currentRepairSpeedCost);

            currentRepairSpeedLevel++;

            float reductionFactor = 1f - (repairSpeedCostMult * currentRepairSpeedLevel);

            Truck.globalRestartDelay = Truck.baseRestartDelay * reductionFactor;

            currentRepairSpeedCost = Mathf.RoundToInt(baseRepairSpeedCost * Mathf.Pow(costMultiplier, currentRepairSpeedLevel));

            UpdateRepairSpeedEntry();
            Debug.Log(Truck.globalRestartDelay);
        }
    }

    private void UpdateRepairSpeedEntry()
    {
        repairSpeedText.text = "Truck Repair Speed (Current Bonus: -" + (100 * repairSpeedCostMult * currentRepairSpeedLevel) + "%)\r\n" +
                               "$" + currentRepairSpeedCost + "\r\n" +
                               "Bonus: -" + (100 * repairSpeedCostMult) + "% Repair Speed";
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreScript : MonoBehaviour
{
    [Header("UI")]
    public GameObject storePanel;
    public TextMeshProUGUI repairSpeedText;
    public TextMeshProUGUI durabilityText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI biggerCargoShipText;

    [Header("Cost Multiplier")]
    public float costMultiplier = 1.75f;    // Price increase each upgrade

    [Header("Repair Speed Upgrade Settings")]
    public int baseRepairSpeedCost = 150;
    public float repairSpeedMult = 0.1f;  // Each upgrade reduces base repair speed by 10% of original
    public int maxRepairSpeedLevel = 5;

    private int currentRepairSpeedLevel = 0;
    private int currentRepairSpeedCost;

    [Header("Durability Upgrade Settings")]
    public int baseDurabilityCost = 15000;
    public int maxDurabilityLevel = 2;

    private int currentDurabilityLevel = 0;
    private int currentDurabilityCost;

    [Header("Boat Speed Upgrade Settings")]
    public int baseSpeedCost = 3000;
    public float speedMult = 0.1f;  // Each upgrade increases boat speed by 10% of original
    public int maxSpeedLevel = 5;

    private int currentSpeedLevel = 0;
    private int currentSpeedCost;

    [Header("New Ship Purchase Settings")]
    public VehicleSpawnScript vehicleSpawnScript;
    public int biggerCargoShipCost = 30000;
    public GameObject biggerCargoShip;

    private void Start()
    {
        currentRepairSpeedCost = baseRepairSpeedCost;
        currentDurabilityCost = baseDurabilityCost;
        currentSpeedCost = baseSpeedCost;

        UpdateRepairSpeedEntry();
        UpdateDurabilityEntry();
        UpdateSpeedEntry();
        UpdateBiggerCargoShipEntry();
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

            float reductionFactor = 1f - (repairSpeedMult * currentRepairSpeedLevel);

            Truck.globalRestartDelay = Truck.baseRestartDelay * reductionFactor;

            currentRepairSpeedCost = Mathf.RoundToInt(baseRepairSpeedCost * Mathf.Pow(costMultiplier, currentRepairSpeedLevel));

            UpdateRepairSpeedEntry();
        }
    }

    public void PurchaseDurabilityUpgrade()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= currentDurabilityCost
                && currentDurabilityLevel <= maxDurabilityLevel)
        {
            ScoreManager.Instance.UpdateSpendableScore(-currentDurabilityCost);

            currentDurabilityLevel++;

            LivesManager.Instance.AddLife();

            currentDurabilityCost += baseDurabilityCost;

            UpdateDurabilityEntry();
        }
    }

    public void PurchaseSpeedUpgrade()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= currentSpeedCost
                && currentSpeedLevel <= maxSpeedLevel)
        {
            ScoreManager.Instance.UpdateSpendableScore(-currentSpeedCost);

            currentSpeedLevel++;

            float increaseFactor = 1f + (speedMult * currentSpeedLevel);

            LineFollow.boatSpeed = LineFollow.speed * increaseFactor;

            currentSpeedCost = Mathf.RoundToInt(baseSpeedCost * Mathf.Pow(costMultiplier, currentSpeedLevel));

            Debug.Log(LineFollow.boatSpeed);
            UpdateSpeedEntry();
        }
    }

    public void PurchaseBiggerCargoShip()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= biggerCargoShipCost)
        {
            ScoreManager.Instance.UpdateSpendableScore(-biggerCargoShipCost);

            vehicleSpawnScript.UnlockShip(biggerCargoShip);
        }
    }

    private void UpdateRepairSpeedEntry()
    {
        repairSpeedText.text = "Truck Repair Speed (Current Bonus: -" + (100 * repairSpeedMult * currentRepairSpeedLevel) + "%)\r\n" +
                               "$" + currentRepairSpeedCost + "\r\n" +
                               "Bonus: -" + (100 * repairSpeedMult) + "% Repair Speed";
    }

    private void UpdateDurabilityEntry()
    {
        durabilityText.text = "Durability (Current Bonus: +" + currentDurabilityLevel + ")\r\n" +
                              "$" + currentDurabilityCost + "\r\n" +
                              "Bonus: +1 Health";
    }

    private void UpdateSpeedEntry()
    {
        speedText.text = "Speed (Current Bonus: +" + (100 * speedMult * currentSpeedLevel) + "%)\r\n" +
                               "$" + currentSpeedCost + "\r\n" +
                               "Bonus: +" + (100 * speedMult) + "% Speed";
    }

    private void UpdateBiggerCargoShipEntry()
    {
        biggerCargoShipText.text = "Bigger Cargo Ship\r\n" +
                                   "$" + biggerCargoShipCost + "\r\n" +
                                   "Max Capacity: 10 Cargo";
    }
}

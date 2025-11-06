using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class StoreScript : NetworkBehaviour
{
    [Header("UI")]
    public GameObject storePanel;
    public TextMeshProUGUI repairSpeedText;
    public TextMeshProUGUI durabilityText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI bigCargoShipText;
    public TextMeshProUGUI biggerCargoShipText;
    public TextMeshProUGUI whiskeyText;
    public TextMeshProUGUI furnitureText;
    public TextMeshProUGUI industrialEquipmentText;

    [Header("Cost Multiplier")]
    public float costMultiplier = 1.75f;    // Price increase each upgrade

    [Header("Repair Speed Upgrade Settings")]
    public int baseRepairSpeedCost = 150;
    public float repairSpeedMult = 0.1f;  // Each upgrade reduces base repair speed by 10% of original
    public int maxRepairSpeedLevel = 4;

    [SyncVar(hook = nameof(UpdateRepairSpeedEntry))] private int currentRepairSpeedLevel = 0;
    [SyncVar(hook = nameof(UpdateRepairSpeedEntry))] private int currentRepairSpeedCost;

    [Header("Durability Upgrade Settings")]
    public int baseDurabilityCost = 15000;
    public int maxDurabilityLevel = 2;

    [SyncVar(hook = nameof(UpdateDurabilityEntry))] private int currentDurabilityLevel = 0;
    [SyncVar(hook = nameof(UpdateDurabilityEntry))] private int currentDurabilityCost;

    [Header("Boat Speed Upgrade Settings")]
    public int baseSpeedCost = 3000;
    public float speedMult = 0.1f;  // Each upgrade increases boat speed by 10% of original
    public int maxSpeedLevel = 4;

    [SyncVar(hook = nameof(UpdateSpeedEntry))] private int currentSpeedLevel = 0;
    [SyncVar(hook = nameof(UpdateSpeedEntry))] private int currentSpeedCost;

    [Header("New Ship Purchase Settings")]
    public VehicleSpawnScript vehicleSpawnScript;

    public int bigCargoShipCost = 12000;
    public GameObject bigCargoShip;

    public int biggerCargoShipCost = 30000;
    public GameObject biggerCargoShip;

    [SyncVar(hook = nameof(UpdateBigCargoShipEntry))] private bool bigCargoShipPurchased = false;
    [SyncVar(hook = nameof(UpdateBiggerCargoShipEntry))] private bool biggerCargoShipPurchased = false;

    [Header("New Cargo Purchase Settings")]
    public int whiskeyCost = 1500;
    public CargoType whiskey;

    public int furnitureCost = 5000;
    public CargoType furniture;

    public int industrialEquipmentCost = 10000;
    public CargoType industrialEquipment;

    [SyncVar(hook = nameof(UpdateWhiskeyEntry))] private bool whiskeyPurchased = false;
    [SyncVar(hook = nameof(UpdateFurnitureEntry))] private bool furniturePurchased = false;
    [SyncVar(hook = nameof(UpdateIndustrialEquipmentEntry))] private bool industrialEquipmentPurchased = false;


    private void Start()
    {
        currentRepairSpeedCost = baseRepairSpeedCost;
        currentDurabilityCost = baseDurabilityCost;
        currentSpeedCost = baseSpeedCost;

        /*
        UpdateRepairSpeedEntry();
        UpdateDurabilityEntry();
        UpdateSpeedEntry();
        UpdateBigCargoShipEntry();
        UpdateBiggerCargoShipEntry();
        UpdateWhiskeyEntry();
        UpdateFurnitureEntry();
        UpdateIndustrialEquipmentEntry();
        */
    }

    public void OpenStore()
    {
        storePanel.SetActive(true);
        //Time.timeScale = 0f;
    }

    public void CloseStore()
    {
        storePanel.SetActive(false);
        //Time.timeScale = 1f;
    }

    [Command]
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

            int oldCost = currentRepairSpeedCost;
            currentRepairSpeedCost = Mathf.RoundToInt(baseRepairSpeedCost * Mathf.Pow(costMultiplier, currentRepairSpeedLevel));

            UpdateRepairSpeedEntry(oldCost, currentRepairSpeedCost);
        }
    }

    [Command]
    public void PurchaseDurabilityUpgrade()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= currentDurabilityCost
                && currentDurabilityLevel <= maxDurabilityLevel)
        {
            ScoreManager.Instance.UpdateSpendableScore(-currentDurabilityCost);

            currentDurabilityLevel++;

            LivesManager.Instance.AddLife();

            int oldCost = currentDurabilityCost;
            currentDurabilityCost += baseDurabilityCost;

            UpdateDurabilityEntry(oldCost, currentDurabilityCost);
        }
    }

    [Command]
    public void PurchaseSpeedUpgrade()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= currentSpeedCost
                && currentSpeedLevel <= maxSpeedLevel)
        {
            ScoreManager.Instance.UpdateSpendableScore(-currentSpeedCost);

            currentSpeedLevel++;

            float increaseFactor = 1f + (speedMult * currentSpeedLevel);

            // Apply globally to all LineFollow boats
            LineFollow.globalBoatSpeed = LineFollow.baseBoatSpeed * increaseFactor;

            int oldCost = currentSpeedCost;
            currentSpeedCost = Mathf.RoundToInt(baseSpeedCost * Mathf.Pow(costMultiplier, currentSpeedLevel));

            UpdateSpeedEntry(oldCost, currentSpeedCost);
        }
    }

    [Command]
    public void PurchaseBigCargoShip()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= bigCargoShipCost
                && !bigCargoShipPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-bigCargoShipCost);

            vehicleSpawnScript.UnlockShip(bigCargoShip);

            bool oldStatus = bigCargoShipPurchased;
            bigCargoShipPurchased = true;
            UpdateBigCargoShipEntry(oldStatus, bigCargoShipPurchased);
        }
    }

    [Command]
    public void PurchaseBiggerCargoShip()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= biggerCargoShipCost
                && !biggerCargoShipPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-biggerCargoShipCost);

            vehicleSpawnScript.UnlockShip(biggerCargoShip);

            bool oldStatus = biggerCargoShipPurchased;
            biggerCargoShipPurchased = true;
            UpdateBiggerCargoShipEntry(oldStatus, biggerCargoShipPurchased);
        }
    }

    [Command]
    public void PurchaseWhiskey()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= whiskeyCost
                && !whiskeyPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-whiskeyCost);

            CargoManager.Instance.UnlockCargo(whiskey);

            bool oldStatus = whiskeyPurchased;
            whiskeyPurchased = true;
            UpdateWhiskeyEntry(oldStatus, whiskeyPurchased);
        }
    }

    [Command]
    public void PurchaseFurniture()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= furnitureCost
                && !furniturePurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-furnitureCost);

            CargoManager.Instance.UnlockCargo(furniture);

            bool oldStatus = furniturePurchased;
            furniturePurchased = true;
            UpdateFurnitureEntry(oldStatus, furniturePurchased);
        }
    }

    [Command]
    public void PurchaseIndustrialEquipment()
    {
        if (storePanel.activeSelf
                && ScoreManager.Instance.GetSpendableScore() >= industrialEquipmentCost
                && !industrialEquipmentPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-industrialEquipmentCost);

            CargoManager.Instance.UnlockCargo(industrialEquipment);

            bool oldStatus = industrialEquipmentPurchased;
            industrialEquipmentPurchased = true;
            UpdateIndustrialEquipmentEntry(oldStatus, industrialEquipmentPurchased);
        }
    }

    private void UpdateRepairSpeedEntry(int oldVal = 0, int newVal = 0)
    {
        repairSpeedText.text = "Truck Repair Speed (Current Bonus: -" + (100 * repairSpeedMult * currentRepairSpeedLevel) + "%)\r\n" +
                               "$" + currentRepairSpeedCost + "\r\n" +
                               "Bonus: -" + (100 * repairSpeedMult) + "% Repair Speed";
    }

    private void UpdateDurabilityEntry(int oldVal = 0, int newVal = 0)
    {
        durabilityText.text = "Durability (Current Bonus: +" + currentDurabilityLevel + ")\r\n" +
                              "$" + currentDurabilityCost + "\r\n" +
                              "Bonus: +1 Health";
    }

    private void UpdateSpeedEntry(int oldVal = 0, int newVal = 0)
    {
        speedText.text = "Speed (Current Bonus: +" + (100 * speedMult * currentSpeedLevel) + "%)\r\n" +
                               "$" + currentSpeedCost + "\r\n" +
                               "Bonus: +" + (100 * speedMult) + "% Speed";
    }

    private void UpdateBigCargoShipEntry(bool oldVal, bool newVal)
    {
        if (!bigCargoShipPurchased)
        {
            bigCargoShipText.text = "Big Cargo Ship\r\n" +
                                    "$" + bigCargoShipCost + "\r\n" +
                                    "Max Capacity: 5 Cargo";
        }
        else
        {
            bigCargoShipText.text = "Big Cargo Ship\r\n" +
                                    "Purchased\r\n" +
                                    "Max Capacity: 5 Cargo";
        }
    }

    private void UpdateBiggerCargoShipEntry(bool oldVal, bool newVal)
    {
        if (!biggerCargoShipPurchased)
        {
            biggerCargoShipText.text = "Bigger Cargo Ship\r\n" +
                                       "$" + biggerCargoShipCost + "\r\n" +
                                       "Max Capacity: 10 Cargo";
        }
        else
        {
            biggerCargoShipText.text = "Bigger Cargo Ship\r\n" +
                                       "Purchased\r\n" +
                                       "Max Capacity: 10 Cargo";
        }
    }

    private void UpdateWhiskeyEntry(bool oldVal, bool newVal)
    {
        if (!whiskeyPurchased)
        {
            whiskeyText.text = "Whiskey\r\n" +
                               "$" + whiskeyCost + "\r\n" +
                               "+200/Delivery";
        }
        else
        {
            whiskeyText.text = "Whiskey\r\n" +
                               "Purchased\r\n" +
                               "+200/Delivery";
        }
    }

    private void UpdateFurnitureEntry(bool oldVal, bool newVal)
    {
        if (!furniturePurchased)
        {
            furnitureText.text = "Furniture\r\n" +
                                 "$" + furnitureCost + "\r\n" +
                                 "+500/Delivery";
        }
        else
        {
            furnitureText.text = "Furniture\r\n" +
                                 "Purchased\r\n" +
                                 "+500/Delivery";
        }
    }

    private void UpdateIndustrialEquipmentEntry(bool oldVal, bool newVal)
    {
        if (!industrialEquipmentPurchased)
        {
            industrialEquipmentText.text = "Industrial Equipment\r\n" +
                                           "$" + industrialEquipmentCost + "\r\n" +
                                           "+1,700/Delivery";
        }
        else
        {
            industrialEquipmentText.text = "Industrial Equipment\r\n" +
                                           "Purchased\r\n" +
                                           "+1,700/Delivery";
        }
    }
}

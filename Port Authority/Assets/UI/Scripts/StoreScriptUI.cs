using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections;

public class StoreScriptUI : NetworkBehaviour
{
    [Header("Menus")]
    public CanvasGroup truckUpgradeMenu;
    public CanvasGroup boatUpgradeMenu;
    public CanvasGroup cargoShopMenu;
    private CanvasGroup currentUpgradeMenu;

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

    public static StoreScriptUI Instance;

    [Header("Stat Upgrade Cards")]
    public StatUpgradeCard repairSpeedCard;
    public StatUpgradeCard durabilityCard;
    public StatUpgradeCard speedCard;

    [Header("New Vehicle Cards")]
    public NewVehicleCard bigCargoShipCard;
    public NewVehicleCard biggerCargoShipCard;

    [Header("Product Cards")]
    public ProductCard whiskeyCard;
    public ProductCard furnitureCard;
    public ProductCard industrialEquipmentCard;

    float fadeDuration = 0.2f;


    private void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        currentRepairSpeedCost = baseRepairSpeedCost;
        currentDurabilityCost = baseDurabilityCost;
        currentSpeedCost = baseSpeedCost;

        UpdateRepairSpeedEntry(0, currentRepairSpeedCost);
        UpdateDurabilityEntry(0, currentDurabilityCost);
        UpdateSpeedEntry(0, currentSpeedCost);

        UpdateBigCargoShipEntry(false, bigCargoShipPurchased);
        UpdateBiggerCargoShipEntry(false, biggerCargoShipPurchased);

        UpdateWhiskeyEntry(false, whiskeyPurchased);
        UpdateFurnitureEntry(false, furniturePurchased);
        UpdateIndustrialEquipmentEntry(false, industrialEquipmentPurchased);
    }


    [Command(requiresAuthority = false)]
    public void PurchaseRepairSpeedUpgrade()
    {

        if (
                 ScoreManagerUI.Instance.GetSpendableScore() >= currentRepairSpeedCost
                && currentRepairSpeedLevel <= maxRepairSpeedLevel)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-currentRepairSpeedCost);

            currentRepairSpeedLevel++;

            float reductionFactor = 1f - (repairSpeedMult * currentRepairSpeedLevel);

            Truck.globalRestartDelay = Truck.baseRestartDelay * reductionFactor;

            int oldCost = currentRepairSpeedCost;
            currentRepairSpeedCost = Mathf.RoundToInt(baseRepairSpeedCost * Mathf.Pow(costMultiplier, currentRepairSpeedLevel));

            UpdateRepairSpeedEntry(oldCost, currentRepairSpeedCost);
        }

        if (ScoreManagerUI.Instance.GetSpendableScore() >= currentRepairSpeedCost
                && currentRepairSpeedLevel <= maxRepairSpeedLevel)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-currentRepairSpeedCost);

            currentRepairSpeedLevel++;

            float reductionFactor = 1f - (repairSpeedMult * currentRepairSpeedLevel);

            Truck.globalRestartDelay = Truck.baseRestartDelay * reductionFactor;

            int oldCost = currentRepairSpeedCost;
            currentRepairSpeedCost = Mathf.RoundToInt(baseRepairSpeedCost * Mathf.Pow(costMultiplier, currentRepairSpeedLevel));

            UpdateRepairSpeedEntry(oldCost, currentRepairSpeedCost);
        }
    }

    [Command(requiresAuthority = false)]
    public void PurchaseDurabilityUpgrade()
    {
        if (ScoreManagerUI.Instance.GetSpendableScore() >= currentDurabilityCost
                && currentDurabilityLevel <= maxDurabilityLevel)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-currentDurabilityCost);

            currentDurabilityLevel++;

            LivesManager.Instance.AddLife();

            int oldCost = currentDurabilityCost;
            currentDurabilityCost += baseDurabilityCost;

            UpdateDurabilityEntry(oldCost, currentDurabilityCost);
        }
    }

    [Command(requiresAuthority = false)]
    public void PurchaseSpeedUpgrade()
    {
        if (ScoreManagerUI.Instance.GetSpendableScore() >= currentSpeedCost
                && currentSpeedLevel <= maxSpeedLevel)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-currentSpeedCost);

            currentSpeedLevel++;

            float increaseFactor = 1f + (speedMult * currentSpeedLevel);

            // Apply globally to all LineFollow boats
            LineFollow.globalBaseBoatSpeed *= increaseFactor;

            int oldCost = currentSpeedCost;
            currentSpeedCost = Mathf.RoundToInt(baseSpeedCost * Mathf.Pow(costMultiplier, currentSpeedLevel));

            UpdateSpeedEntry(oldCost, currentSpeedCost);
        }
    }

    [Command(requiresAuthority = false)]
    public void PurchaseBigCargoShip()
    {
        if (ScoreManagerUI.Instance.GetSpendableScore() >= bigCargoShipCost
                && !bigCargoShipPurchased)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-bigCargoShipCost);

            vehicleSpawnScript.UnlockShip(bigCargoShip);

            bigCargoShipPurchased = true;
            bool oldStatus = bigCargoShipPurchased;
            bigCargoShipPurchased = true;
            UpdateBigCargoShipEntry(oldStatus, bigCargoShipPurchased);
        }
    }

    [Command(requiresAuthority = false)]
    public void PurchaseBiggerCargoShip()
    {

        if (ScoreManagerUI.Instance.GetSpendableScore() >= biggerCargoShipCost
                && !biggerCargoShipPurchased)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-biggerCargoShipCost);

            vehicleSpawnScript.UnlockShip(biggerCargoShip);

            bool oldStatus = biggerCargoShipPurchased;
            biggerCargoShipPurchased = true;
            UpdateBiggerCargoShipEntry(oldStatus, biggerCargoShipPurchased);
        }
    }

    [Command(requiresAuthority = false)]
    public void PurchaseWhiskey()
    {
        if (ScoreManagerUI.Instance.GetSpendableScore() >= whiskeyCost
                && !whiskeyPurchased)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-whiskeyCost);

            CargoManager.Instance.UnlockCargo(whiskey);

            bool oldStatus = whiskeyPurchased;
            whiskeyPurchased = true;
            UpdateWhiskeyEntry(oldStatus, whiskeyPurchased);
        }
    }

    [Command(requiresAuthority = false)]
    public void PurchaseFurniture()
    {
        if (ScoreManagerUI.Instance.GetSpendableScore() >= furnitureCost
                && !furniturePurchased)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-furnitureCost);

            CargoManager.Instance.UnlockCargo(furniture);

            bool oldStatus = furniturePurchased;
            furniturePurchased = true;
            UpdateFurnitureEntry(oldStatus, furniturePurchased);
        }
    }

    [Command(requiresAuthority = false)]
    public void PurchaseIndustrialEquipment()
    {
        if (ScoreManagerUI.Instance.GetSpendableScore() >= industrialEquipmentCost
                && !industrialEquipmentPurchased)
        {
            ScoreManagerUI.Instance.UpdateSpendableScore(-industrialEquipmentCost);

            CargoManager.Instance.UnlockCargo(industrialEquipment);

            industrialEquipmentPurchased = true;

            bool oldStatus = industrialEquipmentPurchased;
            UpdateIndustrialEquipmentEntry(oldStatus, industrialEquipmentPurchased);
        }
    }

    private void UpdateRepairSpeedEntry(int oldVal = 0, int newVal = 0)
    {
        string title = "Repair Speed";
        string current = $"(Current Effect: -{100 * repairSpeedMult * currentRepairSpeedLevel}%)";
        string price = $"${currentRepairSpeedCost}";
        string effect = $"Bonus: -{100 * repairSpeedMult}% Repair Speed";

        repairSpeedCard.SetUpgradeInfo(
            title,
            current,
            price,
            effect,
            currentRepairSpeedLevel,
            maxRepairSpeedLevel,
            PurchaseRepairSpeedUpgrade
        );
    }

    private void UpdateDurabilityEntry(int oldVal = 0, int newVal = 0)
    {
        string title = "Durability";
        string current = $"(Current Effect: +{currentDurabilityLevel})";
        string price = $"${currentDurabilityCost}";
        string effect = "Bonus: +1 Health";

        durabilityCard.SetUpgradeInfo(
            title,
            current,
            price,
            effect,
            currentDurabilityLevel,
            maxDurabilityLevel,
            PurchaseDurabilityUpgrade
        );
    }

    private void UpdateSpeedEntry(int oldVal = 0, int newVal = 0)
    {
        string title = "Speed";
        string current = $"(Current Effect: +{100 * speedMult * currentSpeedLevel}%)";
        string price = $"${currentSpeedCost}";
        string effect = $"Bonus: +{100 * speedMult}% Speed";

        speedCard.SetUpgradeInfo(
            title,
            current,
            price,
            effect,
            currentSpeedLevel,
            maxSpeedLevel,
            PurchaseSpeedUpgrade
        );
    }

    private void UpdateBigCargoShipEntry(bool oldVal, bool newVal)
    {
        bigCargoShipCard.SetUpgradeInfo(
            "Big Cargo Ship",
            $"${bigCargoShipCost}",
            "Max Cargo: 5",
            PurchaseBigCargoShip,
            isPurchased: bigCargoShipPurchased
        );
    }

    private void UpdateBiggerCargoShipEntry(bool oldVal, bool newVal)
    {
        biggerCargoShipCard.SetUpgradeInfo(
            "Bigger Cargo Ship",
            $"${biggerCargoShipCost}",
            "Max Cargo: 10",
            PurchaseBiggerCargoShip,
            isPurchased: biggerCargoShipPurchased
        );
    }

    private void UpdateWhiskeyEntry(bool oldVal, bool newVal)
    {
        whiskeyCard.SetUpgradeInfo(
            "Whiskey",
            $"${whiskeyCost}",
            "+200/Delivery",
            PurchaseWhiskey,
            isPurchased: whiskeyPurchased
        );
    }

    private void UpdateFurnitureEntry(bool oldVal, bool newVal)
    {
        furnitureCard.SetUpgradeInfo(
            "Furniture",
            $"${furnitureCost}",
            "+500/Delivery",
            PurchaseFurniture,
            isPurchased: furniturePurchased
        );
    }

    private void UpdateIndustrialEquipmentEntry(bool oldVal, bool newVal)
    {
        industrialEquipmentCard.SetUpgradeInfo(
            "Industrial Equipment",
            $"${industrialEquipmentCost}",
            "+1,700/Delivery",
            PurchaseIndustrialEquipment,
            isPurchased: industrialEquipmentPurchased
        );
    }

    public void OpenCargoShop()
    {
        cargoShopMenu.gameObject.SetActive(true);
    }

    public void OpenCurrentUpgradeMenu()
    {
        currentUpgradeMenu.gameObject.SetActive(true);
    }

    public void OpenBoatUpgradeMenu()
    {
        currentUpgradeMenu = boatUpgradeMenu;
        boatUpgradeMenu.alpha = 0;
        boatUpgradeMenu.gameObject.SetActive(true);
        StartCoroutine(FadeMenuInAndOut(boatUpgradeMenu, truckUpgradeMenu));
    }

    public void OpenTruckUpgradeMenu()
    {
        currentUpgradeMenu = truckUpgradeMenu;
        truckUpgradeMenu.alpha = 0;
        truckUpgradeMenu.gameObject.SetActive(true);
        StartCoroutine(FadeMenuInAndOut(truckUpgradeMenu, boatUpgradeMenu));
    }

    private IEnumerator FadeMenuInAndOut(CanvasGroup cg1, CanvasGroup cg2)
    {

        yield return StartCoroutine(FadeOut(cg2));
        yield return StartCoroutine(FadeIn(cg1));
    }

    private IEnumerator FadeIn(CanvasGroup cg)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup cg)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;

        cg.gameObject.SetActive(false);
    }
}

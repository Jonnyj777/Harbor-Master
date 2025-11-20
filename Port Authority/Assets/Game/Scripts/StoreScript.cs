using UnityEngine;
using System.Collections;

public class StoreScript : MonoBehaviour
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

    private int currentRepairSpeedLevel = 0;
    private int currentRepairSpeedCost;

    [Header("Loading Speed Upgrade Settings")]
    public int baseLoadingSpeedCost = 1000;
    public float loadingSpeedMult = 0.5f;   // Each upgrade reduces current loading speed by 50%
    public int maxLoadingSpeedLevel = 2;

    private int currentLoadingSpeedLevel = 0;
    private int currentLoadingSpeedCost;

    [Header("Durability Upgrade Settings")]
    public int baseDurabilityCost = 15000;
    public int maxDurabilityLevel = 2;

    private int currentDurabilityLevel = 0;
    private int currentDurabilityCost;

    [Header("Boat Speed Upgrade Settings")]
    public int baseSpeedCost = 3000;
    public float speedMult = 0.1f;  // Each upgrade increases boat speed by 10% of original
    public int maxSpeedLevel = 4;

    private int currentSpeedLevel = 0;
    private int currentSpeedCost;

    [Header("New Ship Purchase Settings")]
    public VehicleSpawnScript vehicleSpawnScript;

    public int bigCargoShipCost = 12000;
    public GameObject bigCargoShip;

    public int biggerCargoShipCost = 30000;
    public GameObject biggerCargoShip;

    private bool bigCargoShipPurchased = false;
    private bool biggerCargoShipPurchased = false;

    [Header("New Cargo Purchase Settings")]
    public int whiskeyCost = 1500;
    public CargoType whiskey;

    public int furnitureCost = 5000;
    public CargoType furniture;

    public int industrialEquipmentCost = 10000;
    public CargoType industrialEquipment;

    private bool whiskeyPurchased = false;
    private bool furniturePurchased = false;
    private bool industrialEquipmentPurchased = false;

    [Header("Stat Upgrade Cards")]
    public StatUpgradeCard repairSpeedCard;
    public StatUpgradeCard loadingSpeedCard;
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
        currentUpgradeMenu = boatUpgradeMenu;
        currentRepairSpeedCost = baseRepairSpeedCost;
        currentLoadingSpeedCost = baseLoadingSpeedCost;
        currentDurabilityCost = baseDurabilityCost;
        currentSpeedCost = baseSpeedCost;

        UpdateRepairSpeedEntry();
        UpdateLoadingSpeedEntry();
        UpdateDurabilityEntry();
        UpdateSpeedEntry();

        UpdateBigCargoShipEntry();
        UpdateBiggerCargoShipEntry();

        UpdateWhiskeyEntry();
        UpdateFurnitureEntry();
        UpdateIndustrialEquipmentEntry();
    }

    // stat upgrades
    public void PurchaseRepairSpeedUpgrade()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= currentRepairSpeedCost
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

    public void PurchaseLoadingSpeedUpgrade()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= currentLoadingSpeedCost
                && currentLoadingSpeedLevel <= maxLoadingSpeedLevel)
        {
            ScoreManager.Instance.UpdateSpendableScore(-currentLoadingSpeedCost);

            currentLoadingSpeedLevel++;

            float reductionFactor = 1f - (loadingSpeedMult * currentLoadingSpeedLevel);
            
            LineFollow.globalTruckLoadingDelay *= reductionFactor;

            currentLoadingSpeedCost = Mathf.RoundToInt(baseLoadingSpeedCost * Mathf.Pow(costMultiplier, currentLoadingSpeedLevel));

            UpdateLoadingSpeedEntry();
        }
    }

    public void PurchaseDurabilityUpgrade()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= currentDurabilityCost
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
        if (ScoreManager.Instance.GetSpendableScore() >= currentSpeedCost
                && currentSpeedLevel <= maxSpeedLevel)
        {
            ScoreManager.Instance.UpdateSpendableScore(-currentSpeedCost);

            currentSpeedLevel++;

            float increaseFactor = 1f + (speedMult * currentSpeedLevel);

            // Apply globally to all LineFollow boats
            LineFollow.globalBaseBoatSpeed *= increaseFactor;

            currentSpeedCost = Mathf.RoundToInt(baseSpeedCost * Mathf.Pow(costMultiplier, currentSpeedLevel));

            UpdateSpeedEntry();
        }
    }

    // boat purchases
    public void PurchaseBigCargoShip()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= bigCargoShipCost
                && !bigCargoShipPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-bigCargoShipCost);

            vehicleSpawnScript.UnlockShip(bigCargoShip);

            bigCargoShipPurchased = true;
            UpdateBigCargoShipEntry();
        }
    }
   
    public void PurchaseBiggerCargoShip()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= biggerCargoShipCost
                && !biggerCargoShipPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-biggerCargoShipCost);

            vehicleSpawnScript.UnlockShip(biggerCargoShip);

            biggerCargoShipPurchased = true;
            UpdateBiggerCargoShipEntry();
        }
    }

    // product purchases
    public void PurchaseWhiskey()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= whiskeyCost
                && !whiskeyPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-whiskeyCost);

            CargoManager.Instance.UnlockCargo(whiskey);

            whiskeyPurchased = true;
            UpdateWhiskeyEntry();
        }
    }

    public void PurchaseFurniture()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= furnitureCost
                && !furniturePurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-furnitureCost);

            CargoManager.Instance.UnlockCargo(furniture);

            furniturePurchased = true;
            UpdateFurnitureEntry();
        }
    }

    public void PurchaseIndustrialEquipment()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= industrialEquipmentCost
                && !industrialEquipmentPurchased)
        {
            ScoreManager.Instance.UpdateSpendableScore(-industrialEquipmentCost);

            CargoManager.Instance.UnlockCargo(industrialEquipment);

            industrialEquipmentPurchased = true;
            UpdateIndustrialEquipmentEntry();
        }
    }

    // update stat upgrades
    private void UpdateRepairSpeedEntry()
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

    private void UpdateLoadingSpeedEntry()
    {
        string title = "Loading Speed";
        string current = $"(Current Effect: -{100 * loadingSpeedMult * currentLoadingSpeedLevel}%)";
        string price = $"${currentLoadingSpeedCost}";
        string effect = $"Bonus: -{100 * loadingSpeedMult}% Load Speed";

        loadingSpeedCard.SetUpgradeInfo(
            title,
            current,
            price,
            effect,
            currentLoadingSpeedLevel,
            maxLoadingSpeedLevel,
            PurchaseLoadingSpeedUpgrade
        );
    }

    private void UpdateDurabilityEntry()
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

    private void UpdateSpeedEntry()
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

    // update boat purchases
    private void UpdateBigCargoShipEntry()
    {
        bigCargoShipCard.SetUpgradeInfo(
            "Big Cargo Ship",
            $"${bigCargoShipCost}",
            "Max Cargo: 5",
            PurchaseBigCargoShip,
            isPurchased: bigCargoShipPurchased
        );
    }

    private void UpdateBiggerCargoShipEntry()
    {
        biggerCargoShipCard.SetUpgradeInfo(
            "Bigger Cargo Ship",
            $"${biggerCargoShipCost}",
            "Max Cargo: 10",
            PurchaseBiggerCargoShip,
            isPurchased: biggerCargoShipPurchased
        );
    }

    // update product purchases
    private void UpdateWhiskeyEntry()
    {
        whiskeyCard.SetUpgradeInfo(
            "Whiskey",
            $"${whiskeyCost}",
            "+200/Delivery",
            PurchaseWhiskey,
            isPurchased: whiskeyPurchased
        );
    }

    private void UpdateFurnitureEntry()
    {
        furnitureCard.SetUpgradeInfo(
            "Furniture",
            $"${furnitureCost}",
            "+500/Delivery",
            PurchaseFurniture,
            isPurchased: furniturePurchased
        );
    }

    private void UpdateIndustrialEquipmentEntry()
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

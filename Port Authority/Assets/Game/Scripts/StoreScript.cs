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

    [Header("Loading Speed Upgrade Settings")]
    public int baseLoadingSpeedCost = 1000;
    public float loadingSpeedMult = 0.5f;   // Each upgrade reduces current loading speed by 50%
    public int maxLoadingSpeedLevel = 2;

    [Header("Durability Upgrade Settings")]
    public int baseDurabilityCost = 15000;
    public int maxDurabilityLevel = 2;

    [Header("Boat Speed Upgrade Settings")]
    public int baseSpeedCost = 3000;
    public float speedMult = 0.1f;  // Each upgrade increases boat speed by 10% of original
    public int maxSpeedLevel = 4;

    [Header("New Ship Purchase Settings")]
    public VehicleSpawnScript vehicleSpawnScript;

    public int bigCargoShipCost = 12000;
    public GameObject bigCargoShip;

    public int biggerCargoShipCost = 30000;
    public GameObject biggerCargoShip;

    [Header("New Cargo Purchase Settings")]
    public int whiskeyCost = 1500;
    public CargoType whiskey;

    public int furnitureCost = 5000;
    public CargoType furniture;

    public int industrialEquipmentCost = 10000;
    public CargoType industrialEquipment;

    
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
    private StoreLogic logic;
    private void Start()
    {
        currentUpgradeMenu = boatUpgradeMenu;

        logic = new StoreLogic(
            costMultiplier,
            baseRepairSpeedCost,
            repairSpeedMult,
            maxRepairSpeedLevel,
            baseLoadingSpeedCost,
            loadingSpeedMult,
            maxLoadingSpeedLevel,
            baseDurabilityCost,
            maxDurabilityLevel,
            baseSpeedCost,
            speedMult,
            maxSpeedLevel,
            bigCargoShipCost,
            biggerCargoShipCost,
            whiskeyCost,
            furnitureCost,
            industrialEquipmentCost
        );

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
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseRepairSpeed(spendable, out int remaining, out float reductionFactor))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            Truck.globalRestartDelay = Truck.baseRestartDelay * reductionFactor;
            UpdateRepairSpeedEntry();
        }
    }

    public void PurchaseLoadingSpeedUpgrade()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseLoadingSpeed(spendable, out int remaining, out float reductionFactor))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            LineFollow.globalTruckLoadingDelay *= reductionFactor;
            UpdateLoadingSpeedEntry();
        }
    }

    public void PurchaseDurabilityUpgrade()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseDurability(spendable, out int remaining))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            LivesManager.Instance.AddLife();
            UpdateDurabilityEntry();
        }
    }

    public void PurchaseSpeedUpgrade()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseSpeed(spendable, out int remaining, out float increaseFactor))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            LineFollow.globalBaseBoatSpeed *= increaseFactor;
            UpdateSpeedEntry();
        }
    }

    // boat purchases
    public void PurchaseBigCargoShip()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseBigCargoShip(spendable, out int remaining))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            vehicleSpawnScript.UnlockShip(bigCargoShip);
            UpdateBigCargoShipEntry();
        }
    }
   
    public void PurchaseBiggerCargoShip()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseBiggerCargoShip(spendable, out int remaining))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            vehicleSpawnScript.UnlockShip(biggerCargoShip);
            UpdateBiggerCargoShipEntry();
        }
    }

    // product purchases
    public void PurchaseWhiskey()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseWhiskey(spendable, out int remaining))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            CargoManager.Instance.UnlockCargo(whiskey);
            UpdateWhiskeyEntry();
        }
    }

    public void PurchaseFurniture()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseFurniture(spendable, out int remaining))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            CargoManager.Instance.UnlockCargo(furniture);
            UpdateFurnitureEntry();
        }
    }

    public void PurchaseIndustrialEquipment()
    {
        int spendable = ScoreManager.Instance.GetSpendableScore();
        if (logic.TryPurchaseIndustrialEquipment(spendable, out int remaining))
        {
            ScoreManager.Instance.UpdateSpendableScore(remaining - spendable);
            CargoManager.Instance.UnlockCargo(industrialEquipment);
            UpdateIndustrialEquipmentEntry();
        }
    }

    // update stat upgrades
    private void UpdateRepairSpeedEntry()
    {
        var state = logic?.RepairSpeedState;
        int currentLevel = state?.CurrentLevel ?? 0;
        int cost = state?.CurrentCost ?? baseRepairSpeedCost;
        string title = "Repair Speed";
        string current = $"(Current Effect: -{100 * repairSpeedMult * currentLevel}%)";
        string price = $"${cost}";
        string effect = $"Bonus: -{100 * repairSpeedMult}% Repair Speed";

        repairSpeedCard.SetUpgradeInfo(
            title, 
            current, 
            price, 
            effect, 
            currentLevel, 
            maxRepairSpeedLevel, 
            PurchaseRepairSpeedUpgrade
        );
    }

    private void UpdateLoadingSpeedEntry()
    {
        var state = logic?.LoadingSpeedState;
        int currentLevel = state?.CurrentLevel ?? 0;
        int cost = state?.CurrentCost ?? baseLoadingSpeedCost;
        string title = "Loading Speed";
        string current = $"(Current Effect: -{100 * loadingSpeedMult * currentLevel}%)";
        string price = $"${cost}";
        string effect = $"Bonus: -{100 * loadingSpeedMult}% Load Speed";

        loadingSpeedCard.SetUpgradeInfo(
            title,
            current,
            price,
            effect,
            currentLevel,
            maxLoadingSpeedLevel,
            PurchaseLoadingSpeedUpgrade
        );
    }

    private void UpdateDurabilityEntry()
    {
        var state = logic?.DurabilityState;
        int currentLevel = state?.CurrentLevel ?? 0;
        int cost = state?.CurrentCost ?? baseDurabilityCost;
        string title = "Durability";
        string current = $"(Current Effect: +{currentLevel})";
        string price = $"${cost}";
        string effect = "Bonus: +1 Health";

        durabilityCard.SetUpgradeInfo(
            title, 
            current, 
            price, 
            effect, 
            currentLevel, 
            maxDurabilityLevel, 
            PurchaseDurabilityUpgrade
        );
    }

    private void UpdateSpeedEntry()
    {
        var state = logic?.SpeedState;
        int currentLevel = state?.CurrentLevel ?? 0;
        int cost = state?.CurrentCost ?? baseSpeedCost;
        string title = "Speed";
        string current = $"(Current Effect: +{100 * speedMult * currentLevel}%)";
        string price = $"${cost}";
        string effect = $"Bonus: +{100 * speedMult}% Speed";

        speedCard.SetUpgradeInfo(
            title, 
            current, 
            price, 
            effect, 
            currentLevel, 
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
            isPurchased: logic?.BigCargoShipState.IsPurchased ?? false
        );
    }

    private void UpdateBiggerCargoShipEntry()
    {
        biggerCargoShipCard.SetUpgradeInfo(
            "Bigger Cargo Ship",
            $"${biggerCargoShipCost}",
            "Max Cargo: 10",
            PurchaseBiggerCargoShip,
            isPurchased: logic?.BiggerCargoShipState.IsPurchased ?? false
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
            isPurchased: logic?.WhiskeyState.IsPurchased ?? false
        );
    }

    private void UpdateFurnitureEntry()
    {
        furnitureCard.SetUpgradeInfo(
            "Furniture",
            $"${furnitureCost}",
            "+500/Delivery",
            PurchaseFurniture,
            isPurchased: logic?.FurnitureState.IsPurchased ?? false
        );
    }

    private void UpdateIndustrialEquipmentEntry()
    {
        industrialEquipmentCard.SetUpgradeInfo(
            "Industrial Equipment",
            $"${industrialEquipmentCost}",
            "+1,700/Delivery",
            PurchaseIndustrialEquipment,
            isPurchased: logic?.IndustrialEquipmentState.IsPurchased ?? false
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

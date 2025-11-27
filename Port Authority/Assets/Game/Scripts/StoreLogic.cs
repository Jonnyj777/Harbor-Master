using System;
using UnityEngine;

/// <summary>
/// Encapsulates all stat/purchase upgrade calculations for the store so the MonoBehaviour stays thin.
/// </summary>
public sealed class StoreLogic
{
    public UpgradeState RepairSpeedState { get; }
    public UpgradeState LoadingSpeedState { get; }
    public UpgradeState DurabilityState { get; }
    public UpgradeState SpeedState { get; }

    public PurchaseState BigCargoShipState { get; }
    public PurchaseState BiggerCargoShipState { get; }
    public PurchaseState WhiskeyState { get; }
    public PurchaseState FurnitureState { get; }
    public PurchaseState IndustrialEquipmentState { get; }

    private readonly float repairSpeedMult;
    private readonly float loadingSpeedMult;
    private readonly float speedMult;

    public StoreLogic(
        float costMultiplier,
        int baseRepairSpeedCost,
        float repairSpeedMult,
        int maxRepairSpeedLevel,
        int baseLoadingSpeedCost,
        float loadingSpeedMult,
        int maxLoadingSpeedLevel,
        int baseDurabilityCost,
        int maxDurabilityLevel,
        int baseSpeedCost,
        float speedMult,
        int maxSpeedLevel,
        int bigCargoShipCost,
        int biggerCargoShipCost,
        int whiskeyCost,
        int furnitureCost,
        int industrialEquipmentCost)
    {
        this.repairSpeedMult = repairSpeedMult;
        this.loadingSpeedMult = loadingSpeedMult;
        this.speedMult = speedMult;

        RepairSpeedState = new UpgradeState(
            baseRepairSpeedCost,
            maxRepairSpeedLevel,
            level => Mathf.RoundToInt(baseRepairSpeedCost * Mathf.Pow(costMultiplier, level))
        );

        LoadingSpeedState = new UpgradeState(
            baseLoadingSpeedCost,
            maxLoadingSpeedLevel,
            level => Mathf.RoundToInt(baseLoadingSpeedCost * Mathf.Pow(costMultiplier, level))
        );

        DurabilityState = new UpgradeState(
            baseDurabilityCost,
            maxDurabilityLevel,
            level => baseDurabilityCost * (level + 1)
        );

        SpeedState = new UpgradeState(
            baseSpeedCost,
            maxSpeedLevel,
            level => Mathf.RoundToInt(baseSpeedCost * Mathf.Pow(costMultiplier, level))
        );

        BigCargoShipState = new PurchaseState(bigCargoShipCost);
        BiggerCargoShipState = new PurchaseState(biggerCargoShipCost);
        WhiskeyState = new PurchaseState(whiskeyCost);
        FurnitureState = new PurchaseState(furnitureCost);
        IndustrialEquipmentState = new PurchaseState(industrialEquipmentCost);
    }

    public bool TryPurchaseRepairSpeed(int spendableScore, out int remainingScore, out float reductionFactor)
    {
        if (!RepairSpeedState.TryPurchase(spendableScore, out remainingScore))
        {
            reductionFactor = 1f;
            return false;
        }

        reductionFactor = 1f - (repairSpeedMult * RepairSpeedState.CurrentLevel);
        return true;
    }

    public bool TryPurchaseLoadingSpeed(int spendableScore, out int remainingScore, out float reductionFactor)
    {
        if (!LoadingSpeedState.TryPurchase(spendableScore, out remainingScore))
        {
            reductionFactor = 1f;
            return false;
        }

        reductionFactor = 1f - (loadingSpeedMult * LoadingSpeedState.CurrentLevel);
        return true;
    }

    public bool TryPurchaseDurability(int spendableScore, out int remainingScore)
    {
        return DurabilityState.TryPurchase(spendableScore, out remainingScore);
    }

    public bool TryPurchaseSpeed(int spendableScore, out int remainingScore, out float increaseFactor)
    {
        if (!SpeedState.TryPurchase(spendableScore, out remainingScore))
        {
            increaseFactor = 1f;
            return false;
        }

        increaseFactor = 1f + (speedMult * SpeedState.CurrentLevel);
        return true;
    }

    public bool TryPurchaseBigCargoShip(int spendableScore, out int remainingScore)
    {
        return BigCargoShipState.TryPurchase(spendableScore, out remainingScore);
    }

    public bool TryPurchaseBiggerCargoShip(int spendableScore, out int remainingScore)
    {
        return BiggerCargoShipState.TryPurchase(spendableScore, out remainingScore);
    }

    public bool TryPurchaseWhiskey(int spendableScore, out int remainingScore)
    {
        return WhiskeyState.TryPurchase(spendableScore, out remainingScore);
    }

    public bool TryPurchaseFurniture(int spendableScore, out int remainingScore)
    {
        return FurnitureState.TryPurchase(spendableScore, out remainingScore);
    }

    public bool TryPurchaseIndustrialEquipment(int spendableScore, out int remainingScore)
    {
        return IndustrialEquipmentState.TryPurchase(spendableScore, out remainingScore);
    }

    public sealed class UpgradeState
    {
        private readonly Func<int, int> nextCostCalculator;

        public int CurrentLevel { get; private set; }
        public int MaxLevel { get; }
        public int CurrentCost { get; private set; }

        public bool IsMaxed => CurrentLevel >= MaxLevel;

        public UpgradeState(int startingCost, int maxLevel, Func<int, int> nextCostCalculator)
        {
            CurrentCost = startingCost;
            MaxLevel = maxLevel;
            this.nextCostCalculator = nextCostCalculator ?? throw new ArgumentNullException(nameof(nextCostCalculator));
        }

        public bool TryPurchase(int spendableScore, out int remainingScore)
        {
            remainingScore = spendableScore;
            if (IsMaxed || spendableScore < CurrentCost)
            {
                return false;
            }

            remainingScore -= CurrentCost;
            CurrentLevel++;
            CurrentCost = nextCostCalculator(CurrentLevel);
            return true;
        }
    }

    public sealed class PurchaseState
    {
        public int Cost { get; }
        public bool IsPurchased { get; private set; }

        public PurchaseState(int cost)
        {
            Cost = cost;
        }

        public bool TryPurchase(int spendableScore, out int remainingScore)
        {
            remainingScore = spendableScore;
            if (IsPurchased || spendableScore < Cost)
            {
                return false;
            }

            IsPurchased = true;
            remainingScore -= Cost;
            return true;
        }
    }
}

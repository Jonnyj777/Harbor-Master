using NUnit.Framework;

public class StoreLogicTests
{
    private StoreLogic CreateLogic()
    {
        return new StoreLogic(
            costMultiplier: 1.75f,
            baseRepairSpeedCost: 150,
            repairSpeedMult: 0.1f,
            maxRepairSpeedLevel: 4,
            baseLoadingSpeedCost: 1000,
            loadingSpeedMult: 0.5f,
            maxLoadingSpeedLevel: 2,
            baseDurabilityCost: 15000,
            maxDurabilityLevel: 2,
            baseSpeedCost: 3000,
            speedMult: 0.1f,
            maxSpeedLevel: 4,
            bigCargoShipCost: 12000,
            biggerCargoShipCost: 30000,
            whiskeyCost: 1500,
            furnitureCost: 5000,
            industrialEquipmentCost: 10000
        );
    }

    [Test]
    public void RepairSpeedPurchase_ReducesDelay()
    {
        var logic = CreateLogic();

        bool purchased = logic.TryPurchaseRepairSpeed(200, out int remaining, out float factor);

        Assert.IsTrue(purchased);
        Assert.AreEqual(50, 200 - remaining);
        Assert.Less(factor, 1f);
        Assert.AreEqual(1, logic.RepairSpeedState.CurrentLevel);
    }

    [Test]
    public void DurabilityPurchase_AddsLife()
    {
        var logic = CreateLogic();

        bool purchased = logic.TryPurchaseDurability(20000, out int remaining);

        Assert.IsTrue(purchased);
        Assert.AreEqual(5000, 20000 - remaining);
        Assert.AreEqual(1, logic.DurabilityState.CurrentLevel);
    }

    [Test]
    public void ProductPurchase_SetsFlag()
    {
        var logic = CreateLogic();

        bool purchased = logic.TryPurchaseWhiskey(2000, out int remaining);

        Assert.IsTrue(purchased);
        Assert.AreEqual(500, remaining);
        Assert.IsTrue(logic.WhiskeyState.IsPurchased);
    }
}

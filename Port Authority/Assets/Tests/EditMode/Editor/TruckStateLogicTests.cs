using NUnit.Framework;

public class TruckStateLogicTests
{
    [Test]
    public void ShouldBounceBack_RequiresWaterAndNoOtherBounce()
    {
        var logic = new TruckStateLogic();

        Assert.IsTrue(logic.ShouldBounceBack(true, false));
        Assert.IsFalse(logic.ShouldBounceBack(true, true));
    }

    [Test]
    public void CanStartPickup_ChecksCapacityAndAvailability()
    {
        var logic = new TruckStateLogic();

        bool canPickup = logic.CanStartPickup(1, 3, false, 2);

        Assert.IsTrue(canPickup);
        Assert.IsFalse(logic.CanStartPickup(3, 3, false, 2));
    }
}

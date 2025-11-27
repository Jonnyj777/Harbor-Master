using NUnit.Framework;

public class NewVehicleCardTests
{
    [Test]
    public void PurchaseState_DisablesButtonWhenOwned()
    {
        var logic = new PurchaseCardLogic();

        PurchaseCardState state = logic.BuildState(isPurchased: true);

        Assert.IsFalse(state.IsInteractable);
        Assert.IsTrue(state.UseBoughtVisual);
        Assert.AreEqual("Owned", state.ButtonLabel);
    }
}

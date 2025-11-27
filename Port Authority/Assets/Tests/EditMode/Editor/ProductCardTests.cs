using NUnit.Framework;

public class ProductCardTests
{
    [Test]
    public void PurchaseState_AllowsInteractionWhenAvailable()
    {
        var logic = new PurchaseCardLogic();

        PurchaseCardState state = logic.BuildState(isPurchased: false);

        Assert.IsTrue(state.IsInteractable);
        Assert.IsFalse(state.UseBoughtVisual);
        Assert.AreEqual("Buy", state.ButtonLabel);
    }
}

using NUnit.Framework;

public class StatUpgradeCardTests
{
    [Test]
    public void BuildState_FlagsMaxedConditions()
    {
        var logic = new StatUpgradeCardLogic();

        StatUpgradeCardState state = logic.BuildState(currentLevel: 3, maxLevel: 3);

        Assert.IsTrue(state.UseBoughtVisual);
        Assert.IsFalse(state.Interactable);
        Assert.AreEqual("Max", state.ButtonLabel);
    }
}

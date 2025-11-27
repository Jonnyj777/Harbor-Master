using NUnit.Framework;

public class ScoreManagerLogicTests
{
    [Test]
    public void AddScore_IncrementsTotals()
    {
        var logic = new ScoreManagerLogic();

        logic.AddScore(100);

        Assert.AreEqual(100, logic.TotalScore);
        Assert.AreEqual(100, logic.SpendableScore);
    }

    [Test]
    public void AdjustSpendable_AppliesDelta()
    {
        var logic = new ScoreManagerLogic();
        logic.AddScore(200);

        logic.AdjustSpendable(-50);

        Assert.AreEqual(150, logic.SpendableScore);
    }
}

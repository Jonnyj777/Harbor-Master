using NUnit.Framework;

public class LivesManagerLogicTests
{
    [Test]
    public void LoseLife_ReturnsTrueAtZero()
    {
        var logic = new LivesManagerLogic(1);

        bool reachedZero = logic.LoseLife();

        Assert.IsTrue(reachedZero);
        Assert.AreEqual(0, logic.Lives);
    }

    [Test]
    public void AddLife_IncrementsCount()
    {
        var logic = new LivesManagerLogic(2);

        logic.AddLife();

        Assert.AreEqual(3, logic.Lives);
    }
}

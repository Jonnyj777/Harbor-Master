using NUnit.Framework;

public class SceneFadeLogicTests
{
    [Test]
    public void CalculateAlpha_LerpsBetweenValues()
    {
        var logic = new SceneFadeLogic();

        float alpha = logic.CalculateAlpha(0f, 1f, elapsedTime: 0.5f, duration: 1f);

        Assert.AreEqual(0.5f, alpha, 0.001f);
    }

    [Test]
    public void CalculateAlpha_HandlesZeroDuration()
    {
        var logic = new SceneFadeLogic();

        float alpha = logic.CalculateAlpha(0f, 1f, elapsedTime: 0.5f, duration: 0f);

        Assert.AreEqual(1f, alpha);
    }
}

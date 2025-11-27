using NUnit.Framework;
using UnityEngine;

public class PlayerCardTests
{
    [Test]
    public void ResolveColor_ReturnsExpectedValue()
    {
        var logic = new PlayerCardVisualLogic();

        Color color = logic.ResolveColor("Blue");

        Assert.AreEqual(new Color(14f / 255f, 165f / 255f, 233f / 255f), color);
    }

    [Test]
    public void ReadyState_ReturnsCorrectLabel()
    {
        var logic = new PlayerCardVisualLogic();

        PlayerReadyState ready = logic.GetReadyState(true);

        Assert.IsTrue(ready.IsReady);
        Assert.AreEqual("Ready", ready.Label);
    }
}

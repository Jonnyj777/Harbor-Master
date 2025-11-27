using NUnit.Framework;
using UnityEngine;

public class CameraControlsLogicTests
{
    [Test]
    public void ClampPosition_RestrictsToBounds()
    {
        var logic = new CameraControlsLogic(5f, 2f, 2f, 0f, 100f, 0f, 50f, 5f, 30f);

        Vector3 clamped = logic.ClampPosition(new Vector3(150f, 0f, -10f));

        Assert.AreEqual(100f, clamped.x);
        Assert.AreEqual(0f, clamped.z);
    }

    [Test]
    public void ApplyPlanarZoom_UsesShiftMultiplier()
    {
        var logic = new CameraControlsLogic(5f, 4f, 3f, -20f, 20f, -20f, 20f, 5f, 30f);
        Vector3 forward = Vector3.forward;

        Vector3 result = logic.ApplyPlanarZoom(Vector3.zero, forward, zoomInput: 1f, deltaTime: 1f, shiftHeld: true);

        Assert.AreEqual(12f, result.z, 0.001f);
    }
}

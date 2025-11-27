using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LineFollowPathLogicTests
{
    [Test]
    public void Simplify_RemovesIntermediatePointWithinTolerance()
    {
        var logic = new LineFollowPathLogic();
        var raw = new List<Vector3>
        {
            Vector3.zero,
            new Vector3(0.5f, 0f, 0.5f),
            Vector3.one
        };

        List<Vector3> simplified = logic.Simplify(raw, epsilon: 1f);

        Assert.AreEqual(2, simplified.Count);
        Assert.AreEqual(Vector3.zero, simplified[0]);
        Assert.AreEqual(Vector3.one, simplified[1]);
    }

    [Test]
    public void Smooth_AddsIntermediatePoints()
    {
        var logic = new LineFollowPathLogic();
        var raw = new List<Vector3> { Vector3.zero, Vector3.one };

        List<Vector3> smoothed = logic.Smooth(raw, iterations: 1);

        Assert.Greater(smoothed.Count, raw.Count);
        Assert.AreEqual(Vector3.zero, smoothed[0]);
        Assert.AreEqual(Vector3.one, smoothed[^1]);
    }
}

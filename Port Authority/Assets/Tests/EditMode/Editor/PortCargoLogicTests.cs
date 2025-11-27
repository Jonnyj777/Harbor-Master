using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PortCargoLogicTests
{
    [Test]
    public void ComputeSpawnPosition_RespectsBounds()
    {
        var logic = new PortCargoLogic();
        var random = new TestRandomProvider();
        random.EnqueueFloat(2f);
        random.EnqueueFloat(3f);

        Vector3 min = Vector3.zero;
        Vector3 max = new Vector3(10f, 5f, 10f);
        Vector3 spawn = logic.ComputeSpawnPosition(min, max, 1f, random);

        Assert.GreaterOrEqual(spawn.x, 1f);
        Assert.LessOrEqual(spawn.x, 9f);
        Assert.AreEqual(max.y + 1f, spawn.y);
        Assert.GreaterOrEqual(spawn.z, 1f);
        Assert.LessOrEqual(spawn.z, 9f);
    }

    [Test]
    public void TryRemoveCargo_ReturnsIndex()
    {
        var logic = new PortCargoLogic();
        var list = new List<Cargo> { new Cargo(), new Cargo() };
        Cargo target = list[1];

        bool removed = logic.TryRemoveCargo(list, target, out int index);

        Assert.IsTrue(removed);
        Assert.AreEqual(1, index);
        Assert.AreEqual(1, list.Count);
    }
}

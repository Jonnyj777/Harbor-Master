using NUnit.Framework;
using UnityEngine;

public class BoatTests
{
    private static readonly BoatWorldBounds bounds = new BoatWorldBounds(0f, 10f, 0f, 10f);

    [Test]
    public void ApplyBounds_DestroysBoat_WhenOutsideBounds()
    {
        var logic = new BoatBehaviourLogic(bounds, buffer: 1f);
        var wrapper = new TestBoatWrapper();

        bool destructionRequested = logic.ApplyBounds(wrapper, new Vector3(20f, 0f, 5f));

        Assert.IsTrue(destructionRequested, "Logic should report that it requested a destruction.");
        Assert.IsTrue(wrapper.DestroyCalled, "Wrapper should receive the destroy callback for out of bounds boats.");
    }

    [Test]
    public void ApplyBounds_DoesNotDestroy_WhenInsideBounds()
    {
        var logic = new BoatBehaviourLogic(bounds, buffer: 1f);
        var wrapper = new TestBoatWrapper();

        bool destructionRequested = logic.ApplyBounds(wrapper, new Vector3(5f, 0f, 5f));

        Assert.IsFalse(destructionRequested);
        Assert.IsFalse(wrapper.DestroyCalled);
    }

    [Test]
    public void ApplyBounds_SkipsDestroy_WhenBoatAlreadyCrashed()
    {
        var logic = new BoatBehaviourLogic(bounds, buffer: 1f);
        var wrapper = new TestBoatWrapper
        {
            HasVehicleValue = true,
            HasCrashedValue = true
        };

        bool destructionRequested = logic.ApplyBounds(wrapper, new Vector3(20f, 0f, 5f));

        Assert.IsFalse(destructionRequested);
        Assert.IsFalse(wrapper.DestroyCalled);
    }

    private sealed class TestBoatWrapper : IBoatMonoWrapper
    {
        public bool DestroyCalled { get; private set; }
        public bool HasVehicleValue { get; set; }
        public bool HasCrashedValue { get; set; }

        public bool HasVehicle => HasVehicleValue;
        public bool HasCrashed => HasCrashedValue;

        public void DestroyOutOfBounds()
        {
            DestroyCalled = true;
        }
    }
}

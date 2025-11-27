using NUnit.Framework;
using UnityEngine;

public class CargoTypeTests
{
    [Test]
    public void CargoType_AllowsPropertyAssignment()
    {
        var cargoType = ScriptableObject.CreateInstance<CargoType>();
        cargoType.cargoName = "Gadgets";
        cargoType.basePrice = 500;
        cargoType.color = Color.green;

        Assert.AreEqual("Gadgets", cargoType.cargoName);
        Assert.AreEqual(500, cargoType.basePrice);
        Assert.AreEqual(Color.green, cargoType.color);
    }
}

using NUnit.Framework;
using UnityEngine;

public class CargoTests
{
    [Test]
    public void CargoStoresValues()
    {
        var cargo = new Cargo
        {
            type = "Ore",
            price = 100,
            amount = 2,
            color = Color.red,
            spawnTime = 5f
        };

        Assert.AreEqual("Ore", cargo.type);
        Assert.AreEqual(100, cargo.price);
        Assert.AreEqual(Color.red, cargo.color);
        Assert.AreEqual(5f, cargo.spawnTime);
    }
}

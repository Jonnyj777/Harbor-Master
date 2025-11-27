using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CargoManagerLogicTests
{
    [Test]
    public void EnsureDefaultUnlocked_AddsFirstCargoType()
    {
        var unlocked = new List<CargoType>();
        var logic = new CargoManagerLogic(unlocked);
        var all = new List<CargoType> { ScriptableObject.CreateInstance<CargoType>() };

        logic.EnsureDefaultUnlocked(all);

        Assert.AreEqual(1, unlocked.Count);
        Assert.AreSame(all[0], unlocked[0]);
    }

    [Test]
    public void UnlockCargo_IgnoresDuplicates()
    {
        var cargoType = ScriptableObject.CreateInstance<CargoType>();
        var unlocked = new List<CargoType> { cargoType };
        var logic = new CargoManagerLogic(unlocked);

        bool added = logic.UnlockCargo(cargoType);

        Assert.IsFalse(added);
        Assert.AreEqual(1, unlocked.Count);
    }
}

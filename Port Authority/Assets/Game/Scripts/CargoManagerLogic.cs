using System;
using System.Collections.Generic;

/// <summary>
/// Logic helper for unlocking and querying cargo types without depending on Unity APIs.
/// </summary>
public sealed class CargoManagerLogic
{
    private readonly List<CargoType> unlockedCargo;

    public CargoManagerLogic(List<CargoType> unlockedCargo)
    {
        this.unlockedCargo = unlockedCargo ?? throw new ArgumentNullException(nameof(unlockedCargo));
    }

    public void EnsureDefaultUnlocked(List<CargoType> allCargoTypes)
    {
        if (allCargoTypes == null || allCargoTypes.Count == 0)
        {
            return;
        }

        CargoType defaultType = allCargoTypes[0];
        if (!unlockedCargo.Contains(defaultType))
        {
            unlockedCargo.Add(defaultType);
        }
    }

    public bool UnlockCargo(CargoType cargoType)
    {
        if (cargoType == null || unlockedCargo.Contains(cargoType))
        {
            return false;
        }

        unlockedCargo.Add(cargoType);
        return true;
    }

    public List<CargoType> GetUnlockedCargo()
    {
        return unlockedCargo;
    }
}

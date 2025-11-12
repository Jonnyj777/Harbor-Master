using System.Collections.Generic;
using UnityEngine;

public class CargoManager : MonoBehaviour
{
    public static CargoManager Instance { get; private set; }

    [Header("All Cargo Types")]
    public List<CargoType> allCargoTypes = new List<CargoType>();

    [Header("Unlocked Cargo Types")]
    public List<CargoType> unlockedCargoTypes = new List<CargoType>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Unlock the first/default cargo type
        if (allCargoTypes.Count > 0)
        {
            unlockedCargoTypes.Add(allCargoTypes[0]);
        }
    }

    public void UnlockCargo(CargoType cargoType)
    {
        if (!unlockedCargoTypes.Contains(cargoType))
        {
            unlockedCargoTypes.Add(cargoType);
        }
    }

    public List<CargoType> GetUnlockedCargo()
    {
        return unlockedCargoTypes;
    }
}

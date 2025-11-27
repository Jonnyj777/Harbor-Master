using System.Collections.Generic;
using UnityEngine;

public class CargoManager : MonoBehaviour
{
    public static CargoManager Instance { get; private set; }

    [Header("All Cargo Types")]
    public List<CargoType> allCargoTypes = new List<CargoType>();

    [Header("Unlocked Cargo Types")]
    public List<CargoType> unlockedCargoTypes = new List<CargoType>();

    private CargoManagerLogic logic;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        logic = new CargoManagerLogic(unlockedCargoTypes);
        logic.EnsureDefaultUnlocked(allCargoTypes);
    }

    public void UnlockCargo(CargoType cargoType)
    {
        logic.UnlockCargo(cargoType);
    }

    public List<CargoType> GetUnlockedCargo()
    {
        return logic.GetUnlockedCargo();
    }
}

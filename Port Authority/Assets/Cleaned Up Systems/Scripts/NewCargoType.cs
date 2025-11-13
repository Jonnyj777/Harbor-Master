using UnityEngine;

[CreateAssetMenu(fileName = "NewCargoType", menuName = "Cargo/Cargo Type")]
public class CargoType : ScriptableObject
{
    public string cargoName;
    public Color color;
    public int basePrice;
}

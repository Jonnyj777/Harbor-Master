using UnityEngine;
using Mirror;
using UnityEngine.UIElements;

[System.Serializable]
public class CargoN : NetworkBehaviour
{
    [SyncVar] public string type;
    [SyncVar] public int price;
    [SyncVar] public int amount;
    [SyncVar] public Vector3 colorData;
    [SyncVar] public float timeLimit = 35f;
    [SyncVar] public float spawnTime;

    public CargoN(string type, int amount, Vector3 colorData, float spawnTime, int price, float timeLimit = 35f)
    {
        this.type = type;
        this.price = price;
        this.amount = amount;
        this.colorData = colorData;
        this.timeLimit = timeLimit;
        this.spawnTime = spawnTime;
    }


}

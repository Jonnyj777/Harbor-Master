using UnityEngine;
using Mirror;

[System.Serializable]
public class Cargo : NetworkBehaviour
{
    [SyncVar] public string type;
    [SyncVar] public int price;
    [SyncVar] public int amount;
    [SyncVar] public Color color;
    [SyncVar] public float timeLimit = 35f;
    [SyncVar] public float spawnTime;

    public Cargo(string type, int amount, Color color, float spawnTime, int price, float timeLimit = 35f)
    {
        this.type = type;
        this.price = price;
        this.amount = amount;
        this.color = color;
        this.timeLimit = timeLimit;
        this.spawnTime = spawnTime;
    }
}

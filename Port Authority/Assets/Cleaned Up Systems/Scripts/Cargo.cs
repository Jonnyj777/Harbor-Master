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
}

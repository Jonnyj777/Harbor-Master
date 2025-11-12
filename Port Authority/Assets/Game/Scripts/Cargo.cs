using UnityEngine;

// Runtime data:
// A temporary object representing a specific piece of cargo currently being carried.
[System.Serializable]
public class Cargo
{
    public string type;
    public int price;
    public int amount;
    public Color color;
    public float timeLimit = 35f;
    public float spawnTime;
}

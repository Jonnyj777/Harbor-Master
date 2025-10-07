using UnityEngine;


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

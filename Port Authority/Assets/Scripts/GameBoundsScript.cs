using UnityEngine;

public class GameBoundsScript : MonoBehaviour
{
    public Vector3 worldTopRight = new Vector3(100, 0, 100);
    public Vector3 worldBottomLeft = new Vector3(0, 0, 0);
    public GameObject ocean;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 oceanSize = worldTopRight - worldBottomLeft;
        Vector3 oceanCenter = (worldBottomLeft + oceanSize) / 2;

        ocean.transform.position = oceanCenter;
        ocean.transform.localScale = new Vector3(oceanSize.x / 10f, 1, oceanSize.z / 10f);

        Instantiate(ocean);
    }
}

using System.Threading;
using UnityEngine;

public class BoatMoveScript : MonoBehaviour
{
    public float moveSpeed = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}

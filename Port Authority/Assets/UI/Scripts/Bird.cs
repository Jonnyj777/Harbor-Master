using UnityEngine;

public class Bird : MonoBehaviour
{
    public float speed = -100f;
    public float resetTime = 10f; 
    private float timer = 0f;
    private Vector3 initialPos;
    private Quaternion initialRot;

    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        timer += Time.deltaTime;

        if (timer >= resetTime)
        {
            transform.position = initialPos;
            transform.rotation = initialRot;
            timer = 0f;
        }
    }
}

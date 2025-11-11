using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 60f;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector3 position = transform.position;
        
        if (Input.GetKey(KeyCode.W))
            position += Vector3.forward * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.S))
            position += Vector3.back * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.D))
            position += Vector3.right * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
            position += Vector3.left * moveSpeed * Time.deltaTime;

        position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
        transform.position = position;
    }

    private void HandleZoom()
    {
        Vector3 position = transform.position;
        if (Input.GetKey(KeyCode.E))
            position.y -= zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q))
            position.y += zoomSpeed * Time.deltaTime;
        
        position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
        transform.position = position;
    }
}

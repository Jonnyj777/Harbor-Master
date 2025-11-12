using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 120f;

    [Header("Input Multipliers")]
    [SerializeField] private float shiftMultiplier = 2f;

    private Quaternion spawnRotation;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();

        if (mainCamera == null)
            mainCamera = GetComponentInChildren<Camera>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        spawnRotation = transform.rotation;
    }
    
    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }
    
    private bool IsShiftHeld()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) input.y += 1f;
        if (Input.GetKey(KeyCode.S)) input.y -= 1f;
        if (Input.GetKey(KeyCode.D)) input.x += 1f;
        if (Input.GetKey(KeyCode.A)) input.x -= 1f;

        if (input == Vector2.zero)
            return;

        Transform basis = mainCamera != null ? mainCamera.transform : transform;
        Vector3 forward = Vector3.ProjectOnPlane(basis.up, Vector3.up);
        
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(basis.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;
        
        forward.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude < 0.0001f)
        {
            right = basis.right;
            right.y = 0f;
        }
        right.Normalize();

        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
        
        float speed = moveSpeed * (IsShiftHeld() ? shiftMultiplier : 1f);
        Vector3 position = transform.position + moveDirection * speed * Time.deltaTime;
        position.y = transform.position.y;
        transform.position = position;
    }

    private void HandleZoom()
    {
        float zoomInput = 0f;
        if (Input.GetKey(KeyCode.E)) zoomInput -= 1f;
        if (Input.GetKey(KeyCode.Q)) zoomInput += 1f;

        if (Mathf.Approximately(zoomInput, 0f))
            return;

        if (mainCamera != null && mainCamera.orthographic)
        {
            float zSpeed = zoomSpeed * (IsShiftHeld() ? shiftMultiplier : 1f);
            float newSize = Mathf.Clamp(mainCamera.orthographicSize + zoomInput * zSpeed * Time.deltaTime, minZoom, maxZoom);
            mainCamera.orthographicSize = newSize;
        }
        else
        {
            Transform basis = mainCamera != null ? mainCamera.transform : transform;
            Vector3 planarForward = Vector3.ProjectOnPlane(basis.forward, Vector3.up).normalized;
            float zSpeed = zoomSpeed * (IsShiftHeld() ? shiftMultiplier : 1f);
            transform.position += planarForward * (zoomInput * zSpeed * Time.deltaTime);
        }
    }
}

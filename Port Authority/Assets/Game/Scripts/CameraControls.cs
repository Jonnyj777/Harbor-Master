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
    [SerializeField] private float minX = -500f;
    [SerializeField] private float maxX = 1300f;
    [SerializeField] private float minZ = -500f;
    [SerializeField] private float maxZ = 1300f;
    [SerializeField] private float panPlaneHeight = 0f;

    [Header("Input Multipliers")]
    [SerializeField] private float shiftMultiplier = 2f;

    private const float PanDeltaThresholdSqr = 0.0001f;

    private Quaternion spawnRotation;
    private bool isMousePanning;
    private Vector3 lastPanWorldPosition;

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
        HandleMousePan();
        HandleMovement();
        HandleZoom();
    }
    
    private bool IsShiftHeld()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(1))
            isMousePanning = TryGetPanWorldPosition(Input.mousePosition, out lastPanWorldPosition);
        else if (Input.GetMouseButtonUp(1))
            isMousePanning = false;

        if (!isMousePanning)
            return;

        if (!TryGetPanWorldPosition(Input.mousePosition, out Vector3 currentWorldPosition))
        {
            isMousePanning = false;
            return;
        }

        Vector3 delta = lastPanWorldPosition - currentWorldPosition;
        delta.y = 0f;

        if (delta.sqrMagnitude <= PanDeltaThresholdSqr)
            return;

        Vector3 position = transform.position + delta;
        position.y = transform.position.y;
        transform.position = ClampPosition(position);
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
        transform.position = ClampPosition(position);
    }

    private void HandleZoom()
    {
        float zoomInput = 0f;
        if (Input.GetKey(KeyCode.E)) zoomInput -= 1f;
        if (Input.GetKey(KeyCode.Q)) zoomInput += 1f;
        zoomInput -= Input.mouseScrollDelta.y;

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
            Vector3 position = transform.position + planarForward * (zoomInput * zSpeed * Time.deltaTime);
            transform.position = ClampPosition(position);
        }
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        return position;
    }

    private bool TryGetPanWorldPosition(Vector3 screenPosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        Camera camera = mainCamera != null ? mainCamera : Camera.main;
        if (camera == null)
            return false;

        Plane plane = new Plane(Vector3.up, new Vector3(0f, panPlaneHeight, 0f));
        Ray ray = camera.ScreenPointToRay(screenPosition);
        if (!plane.Raycast(ray, out float enter))
            return false;

        worldPosition = ray.GetPoint(enter);
        return true;
    }
}

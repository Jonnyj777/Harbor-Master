using UnityEngine;

/// <summary>
/// Pure logic container for camera movement/zoom constraints so we can test behaviour without MonoBehaviour dependencies.
/// </summary>
public class CameraControlsLogic
{
    private readonly float moveSpeed;
    private readonly float zoomSpeed;
    private readonly float shiftMultiplier;
    private readonly float minX;
    private readonly float maxX;
    private readonly float minZ;
    private readonly float maxZ;
    private readonly float minZoom;
    private readonly float maxZoom;

    public CameraControlsLogic(
        float moveSpeed,
        float zoomSpeed,
        float shiftMultiplier,
        float minX,
        float maxX,
        float minZ,
        float maxZ,
        float minZoom,
        float maxZoom)
    {
        this.moveSpeed = moveSpeed;
        this.zoomSpeed = zoomSpeed;
        this.shiftMultiplier = shiftMultiplier;
        this.minX = minX;
        this.maxX = maxX;
        this.minZ = minZ;
        this.maxZ = maxZ;
        this.minZoom = minZoom;
        this.maxZoom = maxZoom;
    }

    public float GetMovementSpeed(bool shiftHeld)
    {
        return moveSpeed * (shiftHeld ? shiftMultiplier : 1f);
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        return position;
    }

    public float ClampZoom(float currentZoom)
    {
        return Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    public Vector3 ApplyPlanarZoom(Vector3 currentPosition, Vector3 planarForward, float zoomInput, float deltaTime, bool shiftHeld)
    {
        float speed = zoomSpeed * (shiftHeld ? shiftMultiplier : 1f);
        Vector3 target = currentPosition + planarForward * (zoomInput * speed * deltaTime);
        return ClampPosition(target);
    }
}

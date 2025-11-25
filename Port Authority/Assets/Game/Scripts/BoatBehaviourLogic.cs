using System;
using UnityEngine;

/// <summary>
/// Pure logic container for Boat behaviour so edit-mode tests can run without MonoBehaviour dependencies.
/// </summary>
public class BoatBehaviourLogic
{
    private readonly BoatWorldBounds bounds;
    private readonly float buffer;

    public BoatBehaviourLogic(BoatWorldBounds bounds, float buffer)
    {
        this.bounds = bounds;
        this.buffer = buffer;
    }

    public float Buffer => buffer;
    public BoatWorldBounds Bounds => bounds;

    /// <summary>
    /// Returns true when the provided position is outside of the configured bounds (taking the buffer into account).
    /// </summary>
    public bool IsOutsideBounds(Vector3 position)
    {
        return position.x < bounds.MinX - buffer ||
               position.x > bounds.MaxX + buffer ||
               position.z < bounds.MinZ - buffer ||
               position.z > bounds.MaxZ + buffer;
    }

    /// <summary>
    /// Applies the out-of-bounds policy. Returns true if a destruction was requested.
    /// </summary>
    public bool ApplyBounds(IBoatMonoWrapper wrapper, Vector3 position)
    {
        if (wrapper == null)
        {
            throw new ArgumentNullException(nameof(wrapper));
        }

        if (wrapper.HasVehicle && wrapper.HasCrashed)
        {
            return false;
        }

        if (IsOutsideBounds(position))
        {
            wrapper.DestroyOutOfBounds();
            return true;
        }

        return false;
    }
}

/// <summary>
/// Adapter interface so MonoBehaviours can bridge to the pure behaviour logic.
/// </summary>
public interface IBoatMonoWrapper
{
    bool HasVehicle { get; }
    bool HasCrashed { get; }
    void DestroyOutOfBounds();
}

[Serializable]
public struct BoatWorldBounds
{
    public float MinX;
    public float MaxX;
    public float MinZ;
    public float MaxZ;

    public BoatWorldBounds(float minX, float maxX, float minZ, float maxZ)
    {
        MinX = minX;
        MaxX = maxX;
        MinZ = minZ;
        MaxZ = maxZ;
    }
}

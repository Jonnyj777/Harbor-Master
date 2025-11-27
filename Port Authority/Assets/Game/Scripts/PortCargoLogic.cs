using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Non-Mono helper for tracking cargo at the port and determining spawn positions.
/// </summary>
public sealed class PortCargoLogic
{
    public void ReceiveCargo(List<Cargo> cargoStore, Cargo cargo)
    {
        if (cargo == null)
        {
            return;
        }

        cargoStore.Add(cargo);
    }

    public bool TryRemoveCargo(List<Cargo> cargoStore, Cargo cargo, out int index)
    {
        index = cargoStore.IndexOf(cargo);
        if (index >= 0)
        {
            cargoStore.RemoveAt(index);
            return true;
        }

        return false;
    }

    public Vector3 ComputeSpawnPosition(Vector3 minBounds, Vector3 maxBounds, float spawnOffset, IRandomProvider random)
    {
        float x = random.Range(minBounds.x + spawnOffset, maxBounds.x - spawnOffset);
        float z = random.Range(minBounds.z + spawnOffset, maxBounds.z - spawnOffset);
        float y = maxBounds.y + spawnOffset;
        return new Vector3(x, y, z);
    }
}

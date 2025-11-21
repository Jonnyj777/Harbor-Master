using UnityEngine;

public class Building : MonoBehaviour
{
    public bool isOnStreet = false;
    public bool isTouchingOtherBuilding = false;

    /// <summary>
    /// Updates collision flags for the spawned building.
    /// </summary>
    public bool Validate()
    {
        isOnStreet = false;
        isTouchingOtherBuilding = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders == null || colliders.Length == 0)
            return true;

        foreach (var collider in colliders)
        {
            if (collider == null || !collider.enabled)
                continue;

            Vector3 center;
            Vector3 halfExtents;
            Quaternion rotation = collider.transform.rotation;

            if (!TryGetWorldBox(collider, out center, out halfExtents))
            {
                center = collider.bounds.center;
                halfExtents = collider.bounds.extents;
                rotation = Quaternion.identity;
            }

            Collider[] overlaps = Physics.OverlapBox(center, halfExtents, rotation, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach (Collider overlap in overlaps)
            {
                if (overlap == null || overlap == collider)
                    continue;

                if (overlap.transform != null && overlap.transform.IsChildOf(transform))
                    continue;

                if (!isOnStreet && overlap.CompareTag("Street"))
                    isOnStreet = true;

                if (!isTouchingOtherBuilding && overlap.CompareTag("Building"))
                    isTouchingOtherBuilding = true;

                if (isOnStreet && isTouchingOtherBuilding)
                    break;
            }

            if (isOnStreet && isTouchingOtherBuilding)
                break;
        }

        return !(isOnStreet || isTouchingOtherBuilding);
    }

    private static bool TryGetWorldBox(Collider collider, out Vector3 center, out Vector3 halfExtents)
    {
        if (collider is BoxCollider box)
        {
            center = collider.transform.TransformPoint(box.center);
            Vector3 lossy = collider.transform.lossyScale;
            Vector3 size = Vector3.Scale(box.size, lossy);
            halfExtents = size * 0.5f;
            return true;
        }

        center = Vector3.zero;
        halfExtents = Vector3.zero;
        return false;
    }
}

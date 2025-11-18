using UnityEngine;
using Mirror;

public class CargoMaterialSetter : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnColorChanged))] public Vector3 colorData;
    private void OnColorChanged(Vector3 oldColorData, Vector3 newColorData)
    {
        if (gameObject.TryGetComponent<Renderer>(out var rend))
        {
            rend.material.color = new Color(newColorData.x, newColorData.y, newColorData.z);
        }
    }
}

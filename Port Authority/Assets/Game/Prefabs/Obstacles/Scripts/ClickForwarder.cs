using UnityEngine;

public class ClickForwarder : MonoBehaviour
{
    private MudPuddle parentMud;  // reference to MudPuddle script

    private void Awake()
    {
        parentMud = GetComponentInParent<MudPuddle>();
        if (parentMud == null)
        {
            Debug.LogWarning($"No MudPuddle found in parents of {gameObject.name}");
        }
    }
    private void OnMouseDown()
    {
        if (parentMud != null)
        {
            parentMud.ShowCleanupButton();
        }
    }
}

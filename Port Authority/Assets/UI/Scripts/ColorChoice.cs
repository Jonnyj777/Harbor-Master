using UnityEngine;
using UnityEngine.UI;

public class ColorChoice : MonoBehaviour
{
    [Header("Color Info")]
    public string colorName;         // e.g. "Red", "Blue"
    public Image colorImage;         // the swatch or background image
    public GameObject anchorIcon;    // the little icon that shows when selected

    private ColorChoiceManager manager;

    private void Awake()
    {
        // Hide anchor by default
        if (anchorIcon != null)
            anchorIcon.SetActive(false);
    }

    private void Start()
    {
        // Find parent manager in hierarchy
        manager = GetComponentInParent<ColorChoiceManager>();

        // Hook up button click
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClickColor);
    }

    private void OnClickColor()
    {
        if (manager != null)
            manager.SelectColor(this);
    }

    // Called by manager when this color is selected or deselected
    public void SetSelected(bool selected)
    {
        if (anchorIcon != null)
            anchorIcon.SetActive(selected);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ColorChoiceManager : MonoBehaviour
{
    private List<ColorChoice> colorChoices;
    private ColorChoice selectedChoice;

    private void Awake()
    {
        // Auto-find all child color choices
        colorChoices = new List<ColorChoice>(GetComponentsInChildren<ColorChoice>());
    }

    public void SelectColor(ColorChoice choice)
    {
        // Deselect previous
        if (selectedChoice != null)
            selectedChoice.SetSelected(false);

        // Select new one
        selectedChoice = choice;
        selectedChoice.SetSelected(true);

        Debug.Log("Selected color: " + selectedChoice.colorName);
    }

    // Optional getter if you want to use the selected color later
    public string GetSelectedColorName()
    {
        return selectedChoice != null ? selectedChoice.colorName : null;
    }
}


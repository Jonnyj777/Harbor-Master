using UnityEngine;

/// <summary>
/// Determines what text/colour a PlayerCard should display so we can test it without UI components.
/// </summary>
public sealed class PlayerCardVisualLogic
{
    public Color ResolveColor(string colorName)
    {
        switch (colorName)
        {
            case "Orange":
                return new Color(255f / 255f, 119f / 255f, 0f / 255f);
            case "Blue":
                return new Color(14f / 255f, 165f / 255f, 233f / 255f);
            case "Pink":
                return new Color(255f / 255f, 31f / 255f, 139f / 255f);
            case "Purple":
                return new Color(182f / 255f, 27f / 255f, 243f / 255f);
            case "Red":
                return new Color(233f / 255f, 14f / 255f, 18f / 255f);
            case "Yellow":
                return new Color(255f / 255f, 217f / 255f, 0f / 255f);
            case "Green":
                return new Color(22f / 255f, 218f / 255f, 35f / 255f);
            default:
                return Color.white;
        }
    }

    public PlayerReadyState GetReadyState(bool isReady)
    {
        return new PlayerReadyState
        {
            IsReady = isReady,
            Label = isReady ? "Ready" : "Not Ready"
        };
    }
}

public struct PlayerReadyState
{
    public bool IsReady;
    public string Label;
}

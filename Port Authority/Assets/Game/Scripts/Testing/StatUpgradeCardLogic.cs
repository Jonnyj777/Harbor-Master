using UnityEngine;

/// <summary>
/// Translates upgrade levels into UI state for the stat upgrade cards.
/// </summary>
public sealed class StatUpgradeCardLogic
{
    public StatUpgradeCardState BuildState(int currentLevel, int maxLevel)
    {
        int clampedLevel = Mathf.Clamp(currentLevel, 0, maxLevel);
        bool isMaxed = clampedLevel >= maxLevel;
        return new StatUpgradeCardState
        {
            FilledNotches = clampedLevel,
            TotalNotches = Mathf.Max(0, maxLevel),
            UseBoughtVisual = isMaxed,
            Interactable = !isMaxed,
            ButtonLabel = isMaxed ? "Max" : "Buy"
        };
    }
}

public struct StatUpgradeCardState
{
    public int FilledNotches;
    public int TotalNotches;
    public bool UseBoughtVisual;
    public bool Interactable;
    public string ButtonLabel;
}

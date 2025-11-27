using UnityEngine;

/// <summary>
/// Provides deterministic fade calculations so the edit-mode tests can verify behaviour.
/// </summary>
public sealed class SceneFadeLogic
{
    public float CalculateAlpha(float start, float end, float elapsedTime, float duration)
    {
        if (duration <= 0f)
        {
            return end;
        }

        float t = Mathf.Clamp01(elapsedTime / duration);
        return Mathf.Lerp(start, end, t);
    }
}

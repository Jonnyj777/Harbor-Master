using UnityEngine;

/// <summary>
/// Abstraction over Unity's Random so the pure logic classes can be tested deterministically.
/// </summary>
public interface IRandomProvider
{
    float Range(float minInclusive, float maxInclusive);
    int RangeInt(int minInclusive, int maxExclusive);
}

public sealed class UnityRandomProvider : IRandomProvider
{
    public float Range(float minInclusive, float maxInclusive)
    {
        return Random.Range(minInclusive, maxInclusive);
    }

    public int RangeInt(int minInclusive, int maxExclusive)
    {
        return Random.Range(minInclusive, maxExclusive);
    }
}

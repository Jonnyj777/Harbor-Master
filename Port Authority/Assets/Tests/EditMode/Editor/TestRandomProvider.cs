using System.Collections.Generic;

public sealed class TestRandomProvider : IRandomProvider
{
    private readonly Queue<float> floatValues = new Queue<float>();
    private readonly Queue<int> intValues = new Queue<int>();

    public void EnqueueFloat(float value)
    {
        floatValues.Enqueue(value);
    }

    public void EnqueueInt(int value)
    {
        intValues.Enqueue(value);
    }

    public float Range(float minInclusive, float maxInclusive)
    {
        if (floatValues.Count > 0)
        {
            return floatValues.Dequeue();
        }

        return (minInclusive + maxInclusive) * 0.5f;
    }

    public int RangeInt(int minInclusive, int maxExclusive)
    {
        if (intValues.Count > 0)
        {
            return intValues.Dequeue();
        }

        return minInclusive;
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Path simplification/smoothing helper shared by the line follower and edit mode tests.
/// </summary>
public sealed class LineFollowPathLogic
{
    public List<Vector3> Simplify(List<Vector3> points, float epsilon)
    {
        return RamerDouglasPeucker(points, epsilon);
    }

    public List<Vector3> Smooth(List<Vector3> points, int iterations)
    {
        if (points == null || points.Count < 2)
        {
            return new List<Vector3>(points ?? new List<Vector3>());
        }

        List<Vector3> smoothed = new List<Vector3>(points);

        for (int i = 0; i < iterations; i++)
        {
            List<Vector3> newPoints = new List<Vector3>();
            newPoints.Add(smoothed[0]);

            for (int j = 0; j < smoothed.Count - 1; j++)
            {
                Vector3 p0 = smoothed[j];
                Vector3 p1 = smoothed[j + 1];

                Vector3 q = 0.75f * p0 + 0.25f * p1;
                Vector3 r = 0.25f * p0 + 0.75f * p1;

                newPoints.Add(q);
                newPoints.Add(r);
            }

            newPoints.Add(smoothed[^1]);
            smoothed = newPoints;
        }

        return smoothed;
    }

    private List<Vector3> RamerDouglasPeucker(List<Vector3> points, float epsilon)
    {
        if (points == null)
        {
            return new List<Vector3>();
        }

        if (points.Count < 3)
        {
            return new List<Vector3>(points);
        }

        int index = -1;
        float maxDistance = 0f;

        for (int i = 1; i < points.Count - 1; i++)
        {
            float distance = PerpendicularDistance(points[i], points[0], points[^1]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                index = i;
            }
        }

        if (maxDistance > epsilon)
        {
            List<Vector3> left = RamerDouglasPeucker(points.GetRange(0, index + 1), epsilon);
            List<Vector3> right = RamerDouglasPeucker(points.GetRange(index, points.Count - index), epsilon);

            left.RemoveAt(left.Count - 1);
            left.AddRange(right);
            return left;
        }

        return new List<Vector3> { points[0], points[^1] };
    }

    private float PerpendicularDistance(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        if (lineStart == lineEnd)
        {
            return Vector3.Distance(point, lineStart);
        }

        Vector3 direction = lineEnd - lineStart;
        Vector3 projected = Vector3.Project(point - lineStart, direction.normalized) + lineStart;
        return Vector3.Distance(point, projected);
    }
}

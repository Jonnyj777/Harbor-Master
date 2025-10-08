using UnityEngine;
using UnityEngine.Splines;

public class TruckSplineMovement : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float speed = 100f;

    private float distanceTraveled = 0f;

    private void Start()
    {
        if (splineContainer != null)
        {
            float splineLength = splineContainer.CalculateLength();
            // randomize starting position along spline
            distanceTraveled = Random.Range(0f, splineLength);
        }
    }

    private void Update()
    {
        if (splineContainer == null) return;

        // calculate total spline length
        float splineLength = splineContainer.CalculateLength();

        // move forward on spline
        distanceTraveled += speed * Time.deltaTime;

        // convert distance to normalized t value
        float t = (distanceTraveled % splineLength) / splineLength;

        // get spline position & tangent
        SplineUtility.Evaluate(splineContainer.Spline, t, out var localPosition, out var tangent, out var up);

        // convert local spline position to world position
        Vector3 worldPosition = splineContainer.transform.TransformPoint(localPosition);

        // apply position and rotation
        transform.SetPositionAndRotation(worldPosition, Quaternion.LookRotation(tangent, up));
    }
}

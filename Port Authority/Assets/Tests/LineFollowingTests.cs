using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PathFollowingSceneTests : InputTestFixture
{
    private const float TargetRadius = 0.75f;
    private const float CompletionTolerance = 3.5f;
    private const float TimeoutSeconds = 25f;
    private const float PathPlaneHeight = 0.1f;
    private const int SamplesPerSegment = 10;
    private const float StillnessThresholdSeconds = 1f;

    [UnityTest]
    public IEnumerator VehicleCompletesStraightPath()
    {
        yield return RunScenario(new Vector3(0f, 0f, 19f));
    }

    [UnityTest]
    public IEnumerator VehicleCompletesSingleTurnPath()
    {
        yield return RunScenario(new Vector3(0f, 0f, 19f), new Vector3(19f, 0f, 19f));
    }

    [UnityTest]
    public IEnumerator VehicleCompletesThreeTurnPath()
    {
        yield return RunScenario(
            new Vector3(0f, 0f, 19f),
            new Vector3(19f, 0f, 19f),
            new Vector3(19f, 0f, -19f),
            new Vector3(-19f, 0f, -19f));
    }

    private IEnumerator RunScenario(params Vector3[] corners)
    {
        Assert.That(corners, Is.Not.Null.And.Not.Empty, "A path with at least one waypoint is required.");

        yield return SceneManager.LoadSceneAsync("PathFollowing", LoadSceneMode.Single);
        yield return null;

        ValidateSceneSetup(out var boat, out var camera, out var lineRenderer);

        var pathPoints = BuildPolylinePath(boat.transform.position, corners);

        var mouse = InputSystem.AddDevice<Mouse>();
        MoveMouse(mouse, camera, boat.transform.position);
        InputSystem.Update();
        yield return null;

        Press(mouse.leftButton);
        InputSystem.Update();
        boat.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
        yield return null;

        foreach (var waypoint in pathPoints)
        {
            MoveMouse(mouse, camera, waypoint);
            InputSystem.Update();
            boat.SendMessage("OnMouseDrag", SendMessageOptions.DontRequireReceiver);
            yield return null;
        }

        Release(mouse.leftButton);
        InputSystem.Update();
        boat.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);

        var finalTarget = GetFinalPathPoint(lineRenderer);
        yield return WaitForVehicleToReachTarget(boat, lineRenderer, finalTarget);

        yield return null;

        InputSystem.RemoveDevice(mouse);
    }

    private static void ValidateSceneSetup(out GameObject boat, out Camera camera, out LineRenderer lineRenderer)
    {
        boat = GameObject.Find("LineFollowerBoat");
        Assert.That(boat, Is.Not.Null, "LineFollowerBoat was not found in PathFollowing scene.");

        var followerFound = boat.GetComponents<MonoBehaviour>()
            .Any(component => component != null && component.GetType().Name == "LineFollower");
        Assert.That(followerFound, Is.True, "LineFollower component is missing on the vehicle.");

        camera = Camera.main;
        Assert.That(camera, Is.Not.Null, "MainCamera was not found in the scene.");

        lineRenderer = boat.GetComponent<LineRenderer>();
        Assert.That(lineRenderer, Is.Not.Null, "LineFollowerBoat is missing the LineRenderer required for path drawing.");
    }

    private IEnumerator WaitForVehicleToReachTarget(GameObject boat, LineRenderer lineRenderer, Vector3 targetPoint)
    {
        var startTime = Time.time;
        var previousPosition = boat.transform.position;
        var stillnessTimer = 0f;

        while (Time.time - startTime < TimeoutSeconds)
        {
            var planarDistance = PlanarDistanceToTarget(boat.transform.position, targetPoint);
            if (planarDistance <= TargetRadius)
            {
                yield break;
            }

            if (lineRenderer != null && lineRenderer.positionCount == 0)
            {
                if (planarDistance <= CompletionTolerance)
                {
                    yield break;
                }

                var movementSinceLastFrame = PlanarDistanceToTarget(boat.transform.position, previousPosition);
                if (movementSinceLastFrame < 0.01f)
                {
                    stillnessTimer += Time.deltaTime;
                    if (stillnessTimer >= StillnessThresholdSeconds)
                    {
                        Assert.That(planarDistance, Is.LessThanOrEqualTo(CompletionTolerance),
                            $"Vehicle stopped moving but remained {planarDistance:F2} units from the target.");
                        yield break;
                    }
                }
                else
                {
                    stillnessTimer = 0f;
                }
            }

            previousPosition = boat.transform.position;
            yield return null;
        }

        var finalDistance = PlanarDistanceToTarget(boat.transform.position, targetPoint);
        Assert.Fail($"Vehicle failed to reach target within {TimeoutSeconds} seconds. Final planar distance: {finalDistance:F2}");
    }

    private static Vector3 GetFinalPathPoint(LineRenderer lineRenderer)
    {
        Assert.That(lineRenderer, Is.Not.Null, "LineRenderer reference was lost before measuring the path.");
        Assert.That(lineRenderer.positionCount, Is.GreaterThan(0), "Simulated drag failed to create a path.");
        return ProjectOntoPathPlane(lineRenderer.GetPosition(lineRenderer.positionCount - 1));
    }

    private static IReadOnlyList<Vector3> BuildPolylinePath(Vector3 start, params Vector3[] corners)
    {
        Assert.That(corners, Is.Not.Null.And.Not.Empty, "At least one corner/target must be provided.");

        var waypoints = new List<Vector3>();
        var current = ProjectOntoPathPlane(start);

        foreach (var corner in corners)
        {
            var projectedCorner = ProjectOntoPathPlane(corner);
            for (var i = 1; i <= SamplesPerSegment; i++)
            {
                var t = i / (float)SamplesPerSegment;
                waypoints.Add(Vector3.Lerp(current, projectedCorner, t));
            }

            current = projectedCorner;
        }

        return waypoints;
    }

    private static float PlanarDistanceToTarget(Vector3 current, Vector3 target)
    {
        var planarCurrent = ProjectOntoPathPlane(current);
        var planarTarget = ProjectOntoPathPlane(target);
        return Vector3.Distance(planarCurrent, planarTarget);
    }

    private static Vector3 ProjectOntoPathPlane(Vector3 position)
    {
        return new Vector3(position.x, PathPlaneHeight, position.z);
    }

    private void MoveMouse(Mouse mouse, Camera camera, Vector3 worldPoint)
    {
        var screenPoint = camera.WorldToScreenPoint(worldPoint);
        Set(mouse.position, (Vector2)screenPoint);
    }
}

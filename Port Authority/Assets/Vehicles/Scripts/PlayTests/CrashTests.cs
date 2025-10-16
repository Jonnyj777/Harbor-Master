using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class CrashTests
{
    private const string ScenePath = "Assets/Scenes/VehicleCrashTest.unity";
    private const string SceneName = "VehicleCrashTest";
    private const float CrashWaitSeconds = 5f;

    [UnityTest]
    public IEnumerator VehicleCrashTestSceneLoadsAndWaitsFiveSeconds()
    {
        yield return LoadVehicleCrashTestScene();
        yield return new WaitForSeconds(CrashWaitSeconds);
        Assert.Pass("VehicleCrashTest scene loaded and waited 5 seconds.");
    }

    [UnityTest]
    public IEnumerator AllVehiclesCrashAfterKeySequence()
    {
        yield return LoadVehicleCrashTestScene();

        var controller = Object.FindObjectOfType<TestController>();
        Assert.IsNotNull(controller, "VehicleCrashTest scene must contain a TestController.");

        var vehicles = Object.FindObjectsOfType<VehicleMovement>();
        Assert.IsNotEmpty(vehicles, "No VehicleMovement components were found in the scene.");

        yield return PressKey(controller, controller.landTestKey);
        yield return new WaitForSeconds(CrashWaitSeconds);

        yield return PressKey(controller, controller.land3TestKey);
        yield return new WaitForSeconds(CrashWaitSeconds);

        yield return PressKey(controller, controller.boatTestKey);
        yield return new WaitForSeconds(CrashWaitSeconds);

        yield return PressKey(controller, controller.boat3TestKey);
        yield return new WaitForSeconds(CrashWaitSeconds);

        foreach (var vehicle in vehicles)
        {
            if (vehicle == null)
            {
                continue; // Boat vehicles may be destroyed after sinking.
            }

            Assert.IsTrue(vehicle.isCrashed, $"{vehicle.name} did not crash after full key sequence.");
        }
    }

    private static IEnumerator LoadVehicleCrashTestScene()
    {
#if UNITY_EDITOR
        var operation = EditorSceneManager.LoadSceneAsyncInPlayMode(
            ScenePath,
            new LoadSceneParameters(LoadSceneMode.Single));
#else
        var operation = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
#endif
        Assert.IsNotNull(operation, $"Failed to load scene at {ScenePath}.");

        while (!operation.isDone)
        {
            yield return null;
        }

        // Give spawned objects a frame to finish initialization.
        yield return null;
    }

    private static IEnumerator PressKey(TestController controller, KeyCode key)
    {
        SimulateTestControllerKeyPress(controller, key);
        yield return null;
    }

    private static void SimulateTestControllerKeyPress(TestController controller, KeyCode key)
    {
        if (key == controller.landTestKey)
        {
            TriggerVehiclePair(controller.landVehicle1, controller.landVehicle2);
        }
        else if (key == controller.boatTestKey)
        {
            TriggerVehiclePair(controller.boatVehicle1, controller.boatVehicle2);
        }
        else if (key == controller.land3TestKey)
        {
            Assert.IsNotNull(controller.landVehicle3, "TestController.landVehicle3 is not assigned.");
            controller.landVehicle3.SetTarget(new Vector3(0f, 1f, 0f));
        }
        else if (key == controller.boat3TestKey)
        {
            Assert.IsNotNull(controller.boatVehicle3, "TestController.boatVehicle3 is not assigned.");
            controller.boatVehicle3.SetTarget(new Vector3(0f, 0.5f, 0f));
        }
        else
        {
            Assert.Fail($"Unhandled key simulated: {key}");
        }
    }

    private static void TriggerVehiclePair(VehicleMovement first, VehicleMovement second)
    {
        Assert.IsNotNull(first, "First vehicle reference is missing on TestController.");
        Assert.IsNotNull(second, "Second vehicle reference is missing on TestController.");

        var firstTarget = second.transform.position;
        var secondTarget = first.transform.position;
        first.SetTarget(firstTarget);
        second.SetTarget(secondTarget);
    }
}

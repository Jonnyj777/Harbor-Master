using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class StructurePlacerRepeatLoadTest
{
    private const string SceneName = "TestMapScene";
    private const string StructurePlacerTypeName = "StructurePlacer";
    private const int Iterations = 20;
    private const float SpawnTimeoutSeconds = 5f;

    private static readonly Lazy<Type> StructurePlacerType = new Lazy<Type>(FindStructurePlacerType);
    private static readonly Lazy<FieldInfo> CommercialCountField = new Lazy<FieldInfo>(() => GetStructurePlacerField("commercialCount"));
    private static readonly Lazy<FieldInfo> FactoryCountField = new Lazy<FieldInfo>(() => GetStructurePlacerField("factoryCount"));
    private static readonly Lazy<FieldInfo> DockCountField = new Lazy<FieldInfo>(() => GetStructurePlacerField("dockCount"));

    [UnityTest]
    public IEnumerator StructurePlacer_MaintainsConfiguredCounts()
    {
        AssertStructurePlacerAccessible();

        Scene originalScene = SceneManager.GetActiveScene();
        string originalScenePath = originalScene.path;

        int? expectedCommercial = null;
        int? expectedFactory = null;
        int? expectedDock = null;

        for (int i = 0; i < Iterations; i++)
        {
            yield return LoadScene(SceneName);
            yield return null;
            yield return new WaitForFixedUpdate();

            if (!TryReadStructureCounts(out int configuredCommercial, out int configuredFactory, out int configuredDock))
                Assert.Fail("Unable to locate StructurePlacer to read configured counts.");

            if (!expectedCommercial.HasValue)
            {
                expectedCommercial = configuredCommercial;
                expectedFactory = configuredFactory;
                expectedDock = configuredDock;
            }
            else
            {
                Assert.AreEqual(expectedCommercial.Value, configuredCommercial, "Serialized commercial count changed between loads.");
                Assert.AreEqual(expectedFactory.Value, configuredFactory, "Serialized factory count changed between loads.");
                Assert.AreEqual(expectedDock.Value, configuredDock, "Serialized dock count changed between loads.");
            }

            yield return WaitForExactTagCount("Commercial", expectedCommercial.Value);
            yield return WaitForExactTagCount("Factory", expectedFactory.Value);
            yield return WaitForExactTagCount("Dock", expectedDock.Value);
        }

        if (!string.IsNullOrEmpty(originalScenePath) && originalScenePath != SceneName)
        {
            yield return LoadScene(originalScenePath);

            Scene reloaded = SceneManager.GetSceneByPath(originalScenePath);
            if (reloaded.IsValid())
                SceneManager.SetActiveScene(reloaded);
        }
    }

    private static IEnumerator LoadScene(string scenePathOrName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scenePathOrName, LoadSceneMode.Single);
        while (!loadOperation.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByPath(scenePathOrName);
        if (!loadedScene.IsValid())
            loadedScene = SceneManager.GetSceneByName(scenePathOrName);

        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);
    }

    private static IEnumerator WaitForExactTagCount(string tag, int expected)
    {
        float elapsed = 0f;

        while (elapsed < SpawnTimeoutSeconds)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            if (objects.Length > expected)
                Assert.Fail($"Found {objects.Length} objects with tag '{tag}' which exceeds expected count {expected}.");

            if (objects.Length == expected)
                yield break;

            yield return null;
            elapsed += Time.deltaTime;
        }

        GameObject[] finalObjects = GameObject.FindGameObjectsWithTag(tag);
        Assert.AreEqual(expected, finalObjects.Length, $"Expected {expected} objects with tag '{tag}', but found {finalObjects.Length} after waiting {SpawnTimeoutSeconds} seconds.");
    }

    private static bool TryReadStructureCounts(out int commercial, out int factory, out int dock)
    {
        commercial = factory = dock = 0;

        Type placerType = StructurePlacerType.Value;
        if (placerType == null)
            return false;

        Scene activeScene = SceneManager.GetActiveScene();
        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            Component[] placers = root.GetComponentsInChildren(placerType, true);
            if (placers.Length == 0)
                continue;

            object instance = placers[0];
            commercial = (int)CommercialCountField.Value.GetValue(instance);
            factory = (int)FactoryCountField.Value.GetValue(instance);
            dock = (int)DockCountField.Value.GetValue(instance);
            return true;
        }

        return false;
    }

    private static void AssertStructurePlacerAccessible()
    {
        Assert.IsNotNull(StructurePlacerType.Value, $"Type '{StructurePlacerTypeName}' not found in loaded assemblies.");
        Assert.IsNotNull(CommercialCountField.Value, "commercialCount field not found on StructurePlacer.");
        Assert.IsNotNull(FactoryCountField.Value, "factoryCount field not found on StructurePlacer.");
        Assert.IsNotNull(DockCountField.Value, "dockCount field not found on StructurePlacer.");
    }

    private static Type FindStructurePlacerType()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(SafeGetTypes)
            .FirstOrDefault(type => type != null && type.Name == StructurePlacerTypeName);
    }

    private static Type[] SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray();
        }
        catch
        {
            return Array.Empty<Type>();
        }
    }

    private static FieldInfo GetStructurePlacerField(string fieldName)
    {
        Type placerType = StructurePlacerType.Value;
        return placerType?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BoatSpawningTests
{
    private const string SceneName = "VehicleSpawningTests";
    private const string BoatTag = "Boat";
    private const float SpawnIntervalSeconds = 10f;
    private const float SpawnBufferSeconds = 1f;
    private const int Iterations = 2;

    [UnityTest]
    public IEnumerator BoatsSpawnOnSixSecondIntervals()
    {
        yield return LoadScene(SceneName);

        int initialBoatCount = GameObject.FindGameObjectsWithTag(BoatTag).Length;

        for (int iteration = 1; iteration <= Iterations; iteration++)
        {
            yield return new WaitForSeconds(SpawnIntervalSeconds + SpawnBufferSeconds);
            Debug.Log("Waited For: " + (iteration * (SpawnIntervalSeconds + SpawnBufferSeconds)) + " seconds.");
            GameObject[] boats = GameObject.FindGameObjectsWithTag(BoatTag);
            Debug.Log("BL: " + boats.Length);
            int expectedCount = initialBoatCount + iteration;
            Assert.IsTrue(expectedCount <= boats.Length,
                $"Expected {expectedCount} boats after {iteration} interval(s) but found {boats.Length}.");
        }
    }

    private static IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        Assert.IsNotNull(loadOperation, $"Failed to start loading scene '{sceneName}'.");

        while (!loadOperation.isDone)
            yield return null;

        yield return null; 
    }
}

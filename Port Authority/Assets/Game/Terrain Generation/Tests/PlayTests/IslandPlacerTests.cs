using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SpawnStructsTests
{
    private const string TestSceneName = "IslandPlacerTest";
    private const string TerrainTileTag = "TerrainTile";
    private const int ExpectedIslandCount = 4;

    [UnityTest]
    public IEnumerator IslandPlacementTest()
    {
        yield return LoadIslandScene();

        GameObject[] terrainTiles = GameObject.FindGameObjectsWithTag(TerrainTileTag);
        Assert.AreEqual(ExpectedIslandCount, terrainTiles.Length,
            $"Expected {ExpectedIslandCount} terrain tiles but found {terrainTiles.Length}.");
    }

    [UnityTest]
    public IEnumerator UniqueIslandsTest()
    {
        yield return LoadIslandScene();

        GameObject[] terrainTiles = GameObject.FindGameObjectsWithTag(TerrainTileTag);
        Assert.AreEqual(ExpectedIslandCount, terrainTiles.Length,
            $"Expected {ExpectedIslandCount} terrain tiles but found {terrainTiles.Length}.");

        for (int i = 0; i < terrainTiles.Length; i++)
        {
            for (int j = i + 1; j < terrainTiles.Length; j++)
            {
                Assert.AreNotEqual(terrainTiles[i].name, terrainTiles[j].name,
                    $"Terrain tiles at indices {i} and {j} share the same name '{terrainTiles[i].name}'.");
            }
        }
    }

    private IEnumerator LoadIslandScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != TestSceneName)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(TestSceneName, LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation, $"Failed to start loading scene '{TestSceneName}'.");

            while (!loadOperation.isDone)
                yield return null;
        }

        yield return null; // Ensure scene objects are initialized before tests run
    }
}

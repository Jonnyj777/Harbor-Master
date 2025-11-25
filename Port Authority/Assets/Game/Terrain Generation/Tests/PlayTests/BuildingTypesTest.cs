using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

    public class BuildingTypesTest
    {
        private const string SceneName = "BuildingTypesTest";

        [UnityTest]
        public IEnumerator BuildingTypes_AreSpawnedExactlyOnce()
        {
            yield return LoadScene(SceneName);

            AssertHasSingleTaggedObject("Commercial");
            AssertHasSingleTaggedObject("Factory");
            AssertHasSingleTaggedObject("Dock");
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != sceneName)
            {
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                Assert.IsNotNull(loadOperation, $"Failed to start loading scene '{sceneName}'.");

                while (!loadOperation.isDone)
                    yield return null;
            }

            yield return null; // Allow one frame for scene objects to initialize fully.
        }

        private static void AssertHasSingleTaggedObject(string tag)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
            Assert.AreEqual(1, objectsWithTag.Length,
                $"Expected exactly one GameObject with tag '{tag}' but found {objectsWithTag.Length}.");
        }
    }

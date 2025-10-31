using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Tests
{
    public class VehicleCrashTests
    {
        private const string SceneName = "VehicleCrashScene";
        private const string BoatTag = "Boat";
        private const float MinimumScanDurationSeconds = 5f;
        private const float MaximumScanDurationSeconds = 15f;
        private const float PostCrashWaitSeconds = 15f;

        [UnityTest]
        public IEnumerator BoatIsDestroyedAfterCrashSequence()
        {
            yield return LoadScene(SceneName);

            GameObject firstBoat = null;
            float elapsed = 0f;

            while (elapsed < MaximumScanDurationSeconds && (elapsed < MinimumScanDurationSeconds || firstBoat == null))
            {
                GameObject boat = GameObject.FindWithTag(BoatTag);
                if (firstBoat == null && boat != null)
                    firstBoat = boat;

                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.IsNotNull(firstBoat,
                $"Expected to find at least one GameObject with tag '{BoatTag}' within {MaximumScanDurationSeconds} seconds.");

            Debug.Log("Waiting");
            yield return new WaitForSeconds(PostCrashWaitSeconds);

            Assert.IsTrue(firstBoat == null,
                "Expected the first spawned boat to be destroyed after the crash sequence, but it still exists.");
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation, $"Failed to start loading scene '{sceneName}'.");

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}

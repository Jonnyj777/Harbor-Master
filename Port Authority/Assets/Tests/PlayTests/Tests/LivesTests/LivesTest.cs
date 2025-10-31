using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Tests.LivesTests
{
    public class LivesTest
    {
        private const string SceneName = "LivesTest";
        private const string BoatTag = "Boat";
        private const string GameUICanvasName = "Game UI";
        private const string LosePopupName = "LosePopupText";
        private const float MaximumWaitForBoatsSeconds = 60f;
        private const float MaximumWaitForBoatDestructionSeconds = 30f;
        private const float LosePopupActivationDelaySeconds = 2f;

        [UnityTest]
        public IEnumerator LosePopupAppearsAfterSixthBoatDestroyed()
        {
            
            
            yield return LoadScene(SceneName);

            List<GameObject> trackedBoats = new List<GameObject>(6);
            HashSet<int> trackedBoatIds = new HashSet<int>();
            float elapsed = 0f;

            while (trackedBoats.Count < 6 && elapsed < MaximumWaitForBoatsSeconds)
            {
                GameObject[] boats = GameObject.FindGameObjectsWithTag(BoatTag);

                foreach (GameObject boat in boats)
                {
                    if (boat == null)
                        continue;

                    int instanceId = boat.GetInstanceID();
                    if (trackedBoatIds.Add(instanceId))
                    {
                        Debug.Log("Tracked boat: " + boat.name);
                        trackedBoats.Add(boat);
                        if (trackedBoats.Count == 6)
                            break;
                    }
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.AreEqual(6, trackedBoats.Count,
                $"Expected to capture six boats within {MaximumWaitForBoatsSeconds} seconds, but only captured {trackedBoats.Count}.");

            GameObject sixthBoat = trackedBoats[5];

            elapsed = 0f;

            
            //TODO: FIGURE OUT HOW TO MAKE THIS WORK IN A CLEANER WAY
            while (elapsed < MaximumWaitForBoatDestructionSeconds)
            {
                Debug.Log("Tacking Sixth Boat at time: " + elapsed);
                if (sixthBoat.GetComponent<Boat>().hasCrashed)
                {
                    Debug.Log("Sixth boat has crashed.");
                    yield return new WaitForSeconds(2);
                    
                    Debug.Log("Checking for Lose Popup...");
                    
                    GameObject gameUICanvas = GameObject.Find(GameUICanvasName);
                    Assert.IsNotNull(gameUICanvas, $"Could not find '{GameUICanvasName}' canvas in the scene.");

                    Transform losePopupTransform = gameUICanvas.transform.Find(LosePopupName);
                    Assert.IsNotNull(losePopupTransform,
                        $"Could not find '{LosePopupName}' under '{GameUICanvasName}'.");

                    GameObject losePopup = losePopupTransform.gameObject;
                    Assert.IsTrue(losePopup.activeInHierarchy,
                        "Expected LosePopupText to be active after the sixth boat was destroyed and the delay elapsed.");
                    break;
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            Assert.IsTrue(false);
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

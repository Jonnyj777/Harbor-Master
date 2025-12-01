using System.Collections;
using UnityEngine;

public class MudPuddle : MonoBehaviour
{
    [Header("Mud Settings")]
    public float slowdownMultiplier = 0.3f;  // how much speed reduces
    public float slownessDuration = 10f;  // how long the slowdown lasts

    [Header("Cleanup Settings")]
    public int cleanupCost = 30;
    public GameObject cleanupButtonPrefab;
    public Canvas mudUICanvas;
    private GameObject cleanupButtonInstance;
    private bool isBlocking = false;

    private void OnTriggerEnter(Collider other)
    {
        Truck truck = other.GetComponent<Truck>();
        if (truck != null)
        {
            truck.ApplyMudEffect(slowdownMultiplier, slownessDuration);
        
            if (!isBlocking)
            {
                isBlocking = true;
                ShowCleanupButton();
            }
        }
    }

    public void ShowCleanupButton()
    {
        if (cleanupButtonPrefab == null || mudUICanvas == null)
        {
            return;
        }

        cleanupButtonInstance = Instantiate(cleanupButtonPrefab, mudUICanvas.transform);

        Vector3 offset = new Vector3(0, 25f, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + offset);
        cleanupButtonInstance.transform.position = screenPos;

        MudCleanupButton button = cleanupButtonInstance.GetComponent<MudCleanupButton>();
        button.Initialize(this, cleanupCost);
    }

    //private IEnumerator MudEffect(VehicleMovement vehicle)
    //{
    //    if (vehicle.mudEffected)
    //        yield break;

    //    vehicle.mudEffected = true;

    //    // store original speed
    //    float ogSpeed = vehicle.speed;

    //    // reduce speed
    //    vehicle.speed *= slowdownMultiplier;

    //    // DEBUG TEST
    //    Debug.Log($"{vehicle.name} hit mud. SPeed reduced for {slownessDuration} seconds.");

    //    yield return new WaitForSeconds(slownessDuration);

    //    // restore speed if not crashed
    //    if (!vehicle.isCrashed)
    //    {
    //        vehicle.speed = ogSpeed;
    //        // DEBUG TEST
    //        Debug.Log($"{vehicle.name} recovered from mud and is back to normal speed.");
    //    }

    //    vehicle.mudEffected = false;
    //}

    public IEnumerator CleanMud()
    {
        AudioManager.Instance.PlayLandObstacleCleanup();

        float fadeDuration = 0.5f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    Color c = r.sharedMaterial.color;
                    r.sharedMaterial.color = new Color(c.r, c.g, c.b, alpha);
                }
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}



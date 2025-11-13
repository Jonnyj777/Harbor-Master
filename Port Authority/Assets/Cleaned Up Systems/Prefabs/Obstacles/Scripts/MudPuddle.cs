using System.Collections;
using UnityEngine;

public class MudPuddle : MonoBehaviour
{
    [Header("Mud Settings")]
    public float slowdownMultiplier = 0.3f;  // how much speed reduces
    public float slownessDuration = 10f;  // how long the slowdown lasts

    private void OnTriggerEnter(Collider other)
    {
        VehicleMovement vehicle = other.GetComponent<VehicleMovement>();
        if (vehicle != null && vehicle.vehicleType == VehicleType.Land)
        {
            StartCoroutine(MudEffect(vehicle));
        }
    }

    private IEnumerator MudEffect(VehicleMovement vehicle)
    {
        if (vehicle.mudEffected)
            yield break;

        vehicle.mudEffected = true;

        // store original speed
        float ogSpeed = vehicle.speed;

        // reduce speed
        vehicle.speed *= slowdownMultiplier;

        // DEBUG TEST
        Debug.Log($"{vehicle.name} hit mud. SPeed reduced for {slownessDuration} seconds.");

        yield return new WaitForSeconds(slownessDuration);

        // restore speed if not crashed
        if (!vehicle.isCrashed)
        {
            vehicle.speed = ogSpeed;
            // DEBUG TEST
            Debug.Log($"{vehicle.name} recovered from mud and is back to normal speed.");
        }

        vehicle.mudEffected = false;
    }

    public void OnChildClicked()
    {
        StartCoroutine(CleanMud());
    }

    private IEnumerator CleanMud()
    {
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
                    Color c = r.material.color;
                    r.material.color = new Color(c.r, c.g, c.b, alpha);
                }
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}



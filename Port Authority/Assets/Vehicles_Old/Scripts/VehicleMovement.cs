using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Rendering;

public class VehicleMovement : MonoBehaviour
{
    [Header("Vehicle Settings")]  // Adds labeled header in Inspector for clarity regarding vehicle type and speed
    public VehicleType vehicleType = VehicleType.Boat;  // Default to Boat, can be changed in Inspector
    public float speed = 5f;  // Default, adjustable speed variable for different vehicles

    [Header("Crash Settings")]  // Adds labeled header in Inspector for clarity regarding vehicle crash states
    public bool isCrashed = false;  // Indicates if the vehicle is in a crashed state
    public CrashType crashType = CrashType.None;  // Type of crash, default to None
    public Color landCrashedColor = Color.red;  // Color to red when land vehicles crash
    public Color boatCrashedColor = Color.cyan;  // Color to when boat vehicles crash

    // settings for boat collisions
    public float sinkLength = 0.5f;  // distance the boat sinks down
    public float sinkDuration = 2f;  // time it takes to sink down to the desired length
    public float fadeDelay = 1f;  // time to wait before fading starts
    public float fadeDuration = 2f;  // how long to fully fade out

    // Target position for the vehicle to move towards (moveTo function)
    private Vector3? targetPosition = null;
    private Renderer vehicleRenderer;
    private Color originalColor;  // used for a reset state later on (e.g., repairs on crashed vehicles)

    // settings for mud obstacle collisions with land vehicles
    public bool mudEffected = false;
    

    // enum for crash types
    public enum CrashType
    {
        None,
        Land,
        Boat
    }

    private void Awake()
    {
        vehicleRenderer = GetComponent<Renderer>();
        if (vehicleRenderer != null)
        {
            originalColor = vehicleRenderer.material.color;
        }
    }

    void Update()
    {
        // If the vehicle is crashed, do not process movement
        if (isCrashed)
        {
            return;
        }

        // Decide movement behavior based on vehicle type
        if (vehicleType == VehicleType.Boat)
        {
            if (targetPosition.HasValue)
            {
                MoveTo(targetPosition.Value);
            }
            //else
            //{
                //DefaultBoatMovement();
            //}
        }
        else if (vehicleType == VehicleType.Land)
        {
            if (targetPosition.HasValue)
            {
                MoveTo(targetPosition.Value);
            }
            else
            {
                DefaultLandMovement();
            }
        }

        // USED FOR TESTING PURPOSES ONLY - REMOVE LATER
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //SetTarget(new Vector3(10, 0, 10));
        //}

        // USED FOR TESTING PURPOSES ONLY - REMOVE LATER
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //SetTarget(new Vector3(70, 0, 20));
        //}
    }

    // Sets a new destination for this vehicle
    // Sade can call this function for path-drawing(?)
    public void SetTarget(Vector3 newTarget)
    {
        targetPosition = newTarget;
    }

    // Moves the vehicle towards the target position
    private void MoveTo(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * Time.deltaTime
            );
    }

    // Default movement for boats (e.g., moving forward continuously)
    private void DefaultBoatMovement()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Default movement for land vehicles
    private void DefaultLandMovement()
    {
        // Discuss cycling behavior in next call
        // TEST KEY USED FOR OBSTACLES SCENE (TREE FALLING)
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    #region Collision Handling
    // Collision detection to set crash state
    private void OnCollisionEnter(Collision collision)
    {
        VehicleMovement other = collision.gameObject.GetComponent<VehicleMovement>();
        if (other == null) return;

        // land vehicle crash state
        // if land vehicle collides into another land vehicle, both land vehicles enter land crash state
        // land crash state == stuck in place, no fade out
        if (vehicleType == VehicleType.Land && other.vehicleType == VehicleType.Land)
        {
            EnterCrashState(CrashType.Land);
            other.EnterCrashState(CrashType.Land);
        }

        // boat vehicle crash state
        // if boat vehicle collides into another boat vehicle, both boat vehicles enter boat crash state
        // boat crash state = disappear off map after a few seconds (do NOT act as additional obstacles)
        if (vehicleType == VehicleType.Boat && other.vehicleType == VehicleType.Boat)
        {
            EnterCrashState(CrashType.Boat);
            other.EnterCrashState(CrashType.Boat);
        }
    }
    #endregion

    #region Crash Handling
    // Apply crash state behavior depending on vehicle type
    public void EnterCrashState(CrashType type, System.Action callback = null)
    {
        isCrashed = true;  // switches the vehicle to a crashed state
        crashType = type;  // helps depict what class of vehicle is crashed

        // stop rigidbody immediately to have vehicles stay in place
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        switch (type)
        {
            case CrashType.Land:
                if (vehicleRenderer != null)
                {
                    vehicleRenderer.material.color = landCrashedColor;
                }
                Debug.Log($"{gameObject.name} crashed into another land vehicle and is now an avoidable obstacle");
                callback?.Invoke();
                break;

            case CrashType.Boat:
                if (vehicleRenderer != null)
                {
                    vehicleRenderer.material.color = boatCrashedColor;
                }
                Debug.Log($"{gameObject.name} crashed and will disappear after 3 seconds...");
                StartCoroutine(SinkFadeOut(callback));  // boat will sink, then fade away and disappear
                break;
        }
    }

    // function to make boats sink, fade, then destroyed after crashing into another boat vehicle
    private IEnumerator SinkFadeOut(System.Action callback = null)
    {
        // sinking
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - sinkLength, startPos.z);
        float time = 0f;
        while (time < sinkDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, time / sinkDuration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;

        // wait before fade
        yield return new WaitForSeconds(fadeDelay);

        // fade out
        if (vehicleRenderer != null)
        {
            Material mat = vehicleRenderer.material;
            Color fadeColor = mat.color;
            float fadeElapsed = 0f;

            // ensure material supports transparency
            mat.SetFloat("_Mode", 2);
            mat.color = fadeColor;

            while (fadeElapsed < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                mat.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                fadeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        // destroy game object
        Destroy(gameObject);

        callback?.Invoke();
    }
    #endregion

    #region Whirlpool Interaction
    public void EnterWhirlpool(Transform whirlpoolCenter, float duration, System.Action<VehicleMovement> callback = null)
    {
        if (!isCrashed)
        {
            isCrashed = true;
            crashType = CrashType.Boat;
            StartCoroutine(SuckedInWhirlpool(whirlpoolCenter, duration, callback));
        }
    }

    private IEnumerator SuckedInWhirlpool(Transform center, float duration, System.Action<VehicleMovement> callback)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(center.position.x, center.position.y - sinkLength, center.position.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // move toward center while sinking
            transform.position = Vector3.Lerp(startPos, endPos, t);

            // rotate around center while moving inward
            transform.RotateAround(center.position, Vector3.up, 360f * Time.deltaTime);

            yield return null;
        }

        // once centered, trigger immediate crash for boat(s)
        Destroy(gameObject);

        // notify the whirlpool that this boat is done
        callback?.Invoke(this);
    }

    #region Mud Puddle Interaction
    public void ApplyMudEffect(float slowdownMultiplier, float duration)
    {
        if (mudEffected)
        {
            return;
        }

        StartCoroutine(MudRecovery(slowdownMultiplier, duration));
    }

    private IEnumerator MudRecovery(float slowdownMultiplier, float duration)
    {
        mudEffected = true;
        float originalSpeed = speed;

        speed *= slowdownMultiplier;
        Debug.Log($"{name} hit mud. Speed reduced for {duration} seconds.");

        yield return new WaitForSeconds(duration);

        if (!isCrashed)
        {
            speed = originalSpeed;
            Debug.Log($"{name} recovered from mud and is back to normal speed.");
        }

        mudEffected = false;
    }
    #endregion
}
#endregion
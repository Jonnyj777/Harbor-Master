using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;

public class TreeObstacle : MonoBehaviour
{
    [Header("Tree Settings")]
    public float fallAngle = 90f;  // impulse force to push it over
    public float fallSpeed = 2f;  // speed which tree stands back up
    public Transform road;

    [Header("Cleanup Settings")]
    public int cleanupCost = 50;  // costs $50 to remove tree from road
    private bool isBlocking = false;  // true when fallen

    public GameObject cleanupButtonPrefab;
    public Canvas treesUICanvas;
    private GameObject cleanupButtonInstance;

    private Quaternion uprightRotation;
    private Quaternion fallenRotation;
    private bool fallen = false;
    private bool falling = false;
    private Rigidbody rb;

    private void Start()
    {

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        uprightRotation = transform.rotation;

        fallenRotation = Quaternion.Euler(fallAngle, transform.eulerAngles.y, transform.eulerAngles.z);

        //if (road == null)
        //{
        //Debug.Log("Could not detect road. Falling normally.");
        //fallenRotation = Quaternion.Euler(fallAngle, transform.eulerAngles.y, transform.eulerAngles.z);
        //return;
        //}
        //if (road != null)
        //{
        //Vector3 toRoad = (road.position - transform.position).normalized;
        //toRoad.y = 0;
        //Quaternion towardRoad = Quaternion.LookRotation(toRoad);
        //transform.rotation = towardRoad;

        //uprightRotation = transform.rotation;
        //fallenRotation = uprightRotation * Quaternion.Euler(fallAngle, 0, 0);
        //}
        //else
        //{
        //fallenRotation = Quaternion.Euler(fallAngle, transform.eulerAngles.y, transform.eulerAngles.z);
        //}

        //Collider roadCollider = road.GetComponent<Collider>();
        //if (roadCollider != null)
        //{
        //    // get nearest point on the road to the tree
        //    Vector3 closestPoint = roadCollider.ClosestPoint(transform.position);
        //    Vector3 fallDirection = (closestPoint - transform.position).normalized;
        //    fallDirection.y = 0f;

        //    fallenRotation = Quaternion.LookRotation(fallDirection) * Quaternion.Euler(fallAngle, 0f, 0f);
        //}
        //else
        //{
        //    fallenRotation = Quaternion.Euler(fallAngle, transform.eulerAngles.y, transform.eulerAngles.z);
        //}

        // start coroutine that waits before falling
        StartCoroutine(StandThenFall());
    }

    private IEnumerator StandThenFall()
    {
        // play spawn sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTreeSpawn();

        // stand upright for 5 seconds
        yield return new WaitForSeconds(5f);

        // start fall
        yield return TriggerFall();
    }
    private void Update()
    {
        if (cleanupButtonInstance != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            cleanupButtonInstance.transform.position = screenPos;
        }
    }

    private IEnumerator TriggerFall()
    {
        //Debug.Log("Tree is falling");

        falling = true;

        //AudioManager.Instance.PlayLandObstacleSpawn();
        rb.isKinematic = true;

        float elapsed = 0f;
        float duration = 1f;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * fallSpeed;
            transform.rotation = Quaternion.Slerp(startRot, fallenRotation, elapsed);
            yield return null;
        }

        transform.rotation = fallenRotation;

        falling = false;
        fallen = true;
        isBlocking = true;

        // show cleanup button
        if (cleanupButtonPrefab != null && treesUICanvas != null)
        {
            cleanupButtonInstance = Instantiate(cleanupButtonPrefab, treesUICanvas.transform);

            // position above tree
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            cleanupButtonInstance.transform.position = screenPos;

            TreeCleanupButton buttonScript = cleanupButtonInstance.GetComponent<TreeCleanupButton>();
            buttonScript.Initialize(this, cleanupCost);
        }

        rb.isKinematic = true;
        rb.detectCollisions = true;

        rb.constraints = RigidbodyConstraints.FreezeAll;

        // TEST
        //Debug.Log($"{gameObject.name} has fallen and is blocking the road.");
    }

    public void SetBlocking(bool state)
    {
        isBlocking = state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!fallen) return;

        Truck truck = other.GetComponent<Truck>();
        if (truck != null)
        {
            truck.EnterCrashState(false);
            return;
        }
    }
}

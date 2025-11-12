using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class TreeObstacle : MonoBehaviour
{
    [Header("Tree Settings")]
    public float fallAngle = 90f;  // impulse force to push it over
    public float fallSpeed = 2f;  // speed which tree stands back up
    public Transform road;

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

        if (road == null)
        {
            Debug.Log("Could not detect road. Falling normally.");
            fallenRotation = Quaternion.Euler(fallAngle, transform.eulerAngles.y, transform.eulerAngles.z);
            return;
        }
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

        Collider roadCollider = road.GetComponent<Collider>();
        if (roadCollider != null)
        {
            // get nearest point on the road to the tree
            Vector3 closestPoint = roadCollider.ClosestPoint(transform.position);
            Vector3 fallDirection = (closestPoint - transform.position).normalized;
            fallDirection.y = 0f;

            fallenRotation = Quaternion.LookRotation(fallDirection) * Quaternion.Euler(fallAngle, 0f, 0f);
        }
        else
        {
            fallenRotation = Quaternion.Euler(fallAngle, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private void Update()
    {
        // TEST KEY TO MAKE TREE FALL USING F
        if (Input.GetKeyDown(KeyCode.F) && !fallen && !falling)
        {
            StartCoroutine(TriggerFall());
        }
    }

    private IEnumerator TriggerFall()
    {

        falling = true;

        rb.isKinematic = true;

        float elapsed = 0f;
        Quaternion startRot = transform.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * fallSpeed;
            transform.rotation = Quaternion.Slerp(startRot, fallenRotation, elapsed);
            yield return null;
        }

        transform.rotation = fallenRotation;

        falling = false;
        fallen = true;

        rb.isKinematic = true;
        rb.detectCollisions = true;

        rb.constraints = RigidbodyConstraints.FreezeAll;

        // TEST
        Debug.Log($"{gameObject.name} has fallen and is blocking the road.");
    }

    private void OnMouseDown()
    {
        if (fallen)
        {
            StartCoroutine(ResetTree());
        }
    }

    private IEnumerator ResetTree()
    {
        rb.isKinematic = true;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;

        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, uprightRotation, elapsed);
            yield return null;
        }

        transform.rotation = uprightRotation;
        fallen = false;

        // freeze rigidbody again
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        
        // TEST DEBUG
        Debug.Log($"{gameObject.name} has been reset upright and is ready to fall again.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!fallen) return;

        VehicleMovement vehicle = other.GetComponent<VehicleMovement>();
        if (vehicle != null && vehicle.vehicleType == VehicleType.Land)
        {
            vehicle.EnterCrashState(VehicleMovement.CrashType.Land);
        }
    }
}

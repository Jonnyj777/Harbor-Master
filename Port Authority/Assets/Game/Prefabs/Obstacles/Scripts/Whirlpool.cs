using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Whirlpool : MonoBehaviour
{
    [Header("Whirlpool Settings")]
    public float rotationSpeed = 90f;  // degrees per second for visual spin
    public float totalDuration = 7f;  // how long whirlpool lasts before disappearing
    public float suckDuration = 3f;  // duration for boats to spin to the center

    private List<VehicleMovement> activeBoats = new List<VehicleMovement>();
    private bool readyToDestroy = false; 

    private void Start()
    {
        // destroy whirlpool after its lifetime
        StartCoroutine(LifetimeCoroutine());
    }

    private void Update()
    {
        // rotate the whirlpool visually around its y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        activeBoats.RemoveAll(b => b == null);
    }

    private void OnTriggerEnter(Collider other)
    {
        VehicleMovement boat = other.GetComponent<VehicleMovement>();
        if (boat != null && boat.vehicleType == VehicleType.Boat)
        {
            // prevent duplicates
            if (!activeBoats.Contains(boat))
            {
                activeBoats.Add(boat);
            }
            Debug.Log($"Boat entered whirlpool. Active boats: {activeBoats.Count}");
            boat.EnterWhirlpool(transform, suckDuration, finishedBoat =>
            {
                OnBoatFinished(finishedBoat);
            });
        }
    }

    private void OnBoatFinished(VehicleMovement boat)
    {
        if (activeBoats.Contains(boat))
        {
            activeBoats.Remove(boat);
        }

        Debug.Log($"Boat finished whirlpool. Active boats: {activeBoats.Count}");
        TryDestroyWhirlpool();
    }

    private IEnumerator LifetimeCoroutine()
    {
        // wait for total duration
        yield return new WaitForSeconds(totalDuration);

        // mark as ready to destroy
        readyToDestroy = true;

        // only destroy if no boats are still being suck in
        TryDestroyWhirlpool();
    }

    private void TryDestroyWhirlpool()
    {
        Debug.Log($"Trying to destroy whirlpool. Active boats: {activeBoats.Count}, isDestroying: {readyToDestroy}");

        if (readyToDestroy && activeBoats.Count == 0)
        {
            Destroy(gameObject);
        }
    }
}
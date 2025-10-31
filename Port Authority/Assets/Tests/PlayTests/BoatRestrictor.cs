/*
using System.Collections.Generic;
using UnityEngine;

public class BoatRestrictor : MonoBehaviour
{
    [SerializeField]
    private int maxBoats = 1;

    [SerializeField]
    private string boatTag = "Boat";

    private readonly List<GameObject> trackedBoats = new List<GameObject>();

    private void Update()
    {
        PruneDestroyedBoats();
        EnforceBoatLimit();
    }

    private void PruneDestroyedBoats()
    {
        for (int i = trackedBoats.Count - 1; i >= 0; i--)
        {
            if (trackedBoats[i] == null)
            {
                trackedBoats.RemoveAt(i);
            }
        }
    }

    private void EnforceBoatLimit()
    {
        GameObject[] boats = GameObject.FindGameObjectsWithTag(boatTag);

        foreach (GameObject boat in boats)
        {
            if (boat == null)
                continue;

            if (trackedBoats.Contains(boat))
                continue;

            if (trackedBoats.Count < maxBoats)
            {
                trackedBoats.Add(boat);
            }
            else
            {
                Destroy(boat);
            }
        }
    }
}
*/
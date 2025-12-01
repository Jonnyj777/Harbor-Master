using System.Collections.Generic;
using UnityEngine;

public class StreetGenerationManager : MonoBehaviour
{
    [Header("Street Groups")]
    [SerializeField] private GameObject[] streetGroups;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool useRandomSelection = true;
    [SerializeField, Min(0)] private int fallbackIndex = 0;
    [SerializeField] private int randomSeed = 0;

    private readonly List<GameObject> activatedChildren = new List<GameObject>();
   
    private void Start()
    {
        if (generateOnStart)
        {
            Debug.Log("Spawning Street Groups");
            ActivateStreetGroups();
        }
    }

    [ContextMenu("Activate Street Groups")]
    public void ActivateStreetGroups()
    {
        activatedChildren.Clear();

        if (streetGroups == null || streetGroups.Length == 0)
            return;

        Random.State previousState = Random.state;
        bool seeded = false;
        if (useRandomSelection && randomSeed != 0)
        {
            Random.InitState(randomSeed);
            seeded = true;
        }

        for (int groupIndex = 0; groupIndex < streetGroups.Length; groupIndex++)
        {
            GameObject group = streetGroups[groupIndex];
            group.SetActive(true);
            if (group == null)
                continue;

            Transform groupTransform = group.transform;
            int childCount = groupTransform.childCount;

            if (childCount == 0)
                continue;

            int selectedIndex = GetSelectedIndex(childCount);

            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                GameObject child = groupTransform.GetChild(childIndex).gameObject;
                bool shouldActivate = childIndex == selectedIndex;

                child.SetActive(shouldActivate);

                if (shouldActivate)
                {
                    activatedChildren.Add(child);
                }
            }
        }

        if (seeded)
        {
            Random.state = previousState;
        }
    }

    private int GetSelectedIndex(int childCount)
    {
        if (childCount <= 0)
            return -1;

        if (useRandomSelection)
        {
            return Random.Range(0, childCount);
        }

        return Mathf.Clamp(fallbackIndex, 0, childCount - 1);
    }

    public List<GameObject> GetActivatedChildren()
    {
        return activatedChildren;
    }
}

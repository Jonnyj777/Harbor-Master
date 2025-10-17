using UnityEngine;

public class GameUIManagerScript : MonoBehaviour
{
    public static GameUIManagerScript Instance;

    [Header("UI References")]

    [SerializeField]
    private GameObject crashedTruckPanel;  // Contains a list of crashed trucks

    [SerializeField]
    private GameObject crashedTruckPrefab;   // A prefab with Text + Button to purchase repair

    private List<Truck> crashedTrucks = new List<Truck>();
    private List<GameObject> crashedTruckPrefabs = new List<GameObject>();

    private int repairCost = 50;

    void Awake()
    {
        Instance = this;
    }

    public void AddCrashedTruck(Truck truck)
    {
        crashedTrucks.Add(truck);
        RefreshCrashedTruckPanel();
    }

    public void RemoveCrashedTruck(Truck truck)
    {
        if (ScoreManager.Instance.GetSpendableScore() >= repairCost)
        {
            crashedTrucks.Remove(truck);
            RefreshCrashedTruckPanel();
            ScoreManager.Instance.UpdateSpendableScore(-repairCost);
        }
    }

    private void RefreshPanel()
    {
        // Clear old entries
        foreach (GameObject entry in crashedTruckPrefabs)
        {
            Destroy(entry);
        }
        crashedTruckPrefabs.Clear();

        // Create new entries
        foreach (Truck truck in crashedTrucks)
        {
            GameObject entry = Instantiate(crashedTruckPrefab, crashedTruckPanel.transform);
            Text label = entry.GetComponentInChildren<Text>();
            Button button = entry.GetComponentInChildren<Button>();

            label.text = "Purchase Repair: $" + repairCost;
            button.onClick.AddListener(() => truck.RepairTruck());

            crashedTruckPrefabs.Add(entry);
        }
    }
}

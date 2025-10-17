using UnityEngine;

public class StoreScript : MonoBehaviour
{
    public GameObject storePanel;

    private int repairCost = 50;
    private int repairSpeedCost = 150;

    public void OpenStore()
    {
        storePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseStore()
    {
        storePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PurchaseRepair()
    {
        if (storePanel.activeSelf && ScoreManager.Instance.GetSpendableScore() >= repairCost)
        {

        }
    }

    public void PurchaseRepairSpeedUpgrade()
    {
        if (storePanel.activeSelf && ScoreManager.Instance.GetSpendableScore() >= repairSpeedCost)
        {
            Truck.globalRestartDelay /= 1.5f;
            ScoreManager.Instance.UpdateSpendableScore(-repairSpeedCost);
            Debug.Log(Truck.globalRestartDelay);
        }
    }
}

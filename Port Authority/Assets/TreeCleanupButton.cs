using UnityEngine;
using UnityEngine.UI;

public class TreeCleanupButton : MonoBehaviour
{
    private TreeObstacle tree;
    private int cleanupCost;
    private Button button;

    public void Initialize(TreeObstacle treeObstacle, int cost)
    {
        tree = treeObstacle;
        cleanupCost = cost;
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickCleanup);
    }

    private void OnClickCleanup()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= cleanupCost)
        {
            ScoreManager.Instance.UpdateSpendableScore(-cleanupCost);
            tree.SetBlocking(false);
            AudioManager.Instance.PlayLandObstacleCleanup();
            Destroy(tree.gameObject);
            Destroy(gameObject);
        }
    }
}

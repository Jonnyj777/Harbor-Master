using UnityEngine;
using UnityEngine.UI;

public class MudCleanupButton : MonoBehaviour
{
    private MudPuddle mud;
    private int cleanupCost;
    private Button button;

    public void Initialize(MudPuddle mudPuddle, int cost)
    {
        mud = mudPuddle;
        cleanupCost = cost;
        button = GetComponent<Button>();
        button.onClick.AddListener(OnCleanup);
    }

    private void OnCleanup()
    {
        if (ScoreManager.Instance.GetSpendableScore() >= cleanupCost)
        {
            ScoreManager.Instance.UpdateSpendableScore(-cleanupCost);
            mud.StartCoroutine(mud.CleanMud());
            Destroy(gameObject);
        }
    }
}

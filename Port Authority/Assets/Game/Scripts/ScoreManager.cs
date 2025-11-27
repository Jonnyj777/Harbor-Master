using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI popUpText;
    public TextMeshProUGUI losePopUpText;
    public Transform losePopUp;
    private ScoreManagerLogic logic;

    private void Start()
    {
        UpdateScoreEntry();
    }

    void Awake()
    {
        Instance = this;
        logic = new ScoreManagerLogic();
    }

    public void AddScore(int scoreUpdate, bool bonus)
    {
        logic.AddScore(scoreUpdate);

        UpdateScoreEntry();

        StartCoroutine(ShowPopUp("+$ " + scoreUpdate, bonus));
    }

    private IEnumerator ShowPopUp(string text, bool bonus)
    {
        popUpText.text = text;
        popUpText.gameObject.SetActive(true);

        if (bonus)
        {
            popUpText.text += " (BONUS!)";
        }

        yield return new WaitForSeconds(1f); 

        popUpText.gameObject.SetActive(false);
    }

    public void ShowLosePopUp()
    {
        losePopUp.gameObject.SetActive(true);
        losePopUpText.text = "$" + logic.TotalScore;
    }

    public void UpdateSpendableScore(int scoreUpdate)
    {
        logic.AdjustSpendable(scoreUpdate);

        UpdateScoreEntry();
    }

    public int GetSpendableScore()
    {
        return logic.SpendableScore;
    }

    private void UpdateScoreEntry()
    {
        scoreText.text = "$ " + logic.SpendableScore;
    }
}

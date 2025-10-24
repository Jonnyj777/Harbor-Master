using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int totalScore = 0;
    private int spendableScore = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI popUpText;
    public TextMeshProUGUI losePopUpText;

    private void Start()
    {
        UpdateScoreEntry();
    }

    void Awake()
    {
        Instance = this;
    }

    public void AddScore(int scoreUpdate, bool bonus)
    {
        totalScore += scoreUpdate;
        spendableScore += scoreUpdate;

        UpdateScoreEntry();

        StartCoroutine(ShowPopUp("+$ " + scoreUpdate, bonus));
    }

    private IEnumerator ShowPopUp(string text, bool bonus)
    {
        popUpText.text = text;
        popUpText.gameObject.SetActive(true);

        if (bonus)
        {
            popUpText.text += " Bonus!";
        }

        yield return new WaitForSeconds(1f); 

        popUpText.gameObject.SetActive(false);
    }

    public void ShowLosePopUp()
    {
        losePopUpText.text = "Great Work!\r\n" +
                             "You safely delivered\r\n" +
                             "$" + totalScore + "\r\n" +
                             "worth of goods\r\n" +
                             "this shift.";
        losePopUpText.gameObject.SetActive(true);
    }

    public void UpdateSpendableScore(int scoreUpdate)
    {
        spendableScore += scoreUpdate;

        UpdateScoreEntry();
    }

    public int GetSpendableScore()
    {
        return spendableScore;
    }

    private void UpdateScoreEntry()
    {
        scoreText.text = "$ " + spendableScore;
    }
}

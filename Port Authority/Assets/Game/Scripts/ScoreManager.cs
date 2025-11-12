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
    public Transform losePopUp;

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
            popUpText.text += " (BONUS!)";
        }

        yield return new WaitForSeconds(1f); 

        popUpText.gameObject.SetActive(false);
    }

    public void ShowLosePopUp()
    {
        losePopUp.gameObject.SetActive(true);
        losePopUpText.text = "$" + totalScore;
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

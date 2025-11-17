using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
public class ScoreManagerUI : NetworkBehaviour
{
    public static ScoreManagerUI Instance;
    [SyncVar] private int totalScore = 0;
    [SyncVar(hook = nameof(OnScoreChanged))] private int spendableScore = 0;
    public TextMeshProUGUI losePopUpText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI popUpText;
    public Transform losePopUp;
    public bool hasBonus = false;

    private void Start()
    {
        OnScoreChanged(spendableScore, spendableScore);
    }

    void Awake()
    {
        Instance = this;
    }

    void OnScoreChanged(int oldVal, int newVal)
    {
        scoreText.text = "$ " + newVal;

        StartCoroutine(ShowPopUp("+$ " + (newVal - oldVal), hasBonus));
    }

    public void AddScore(int scoreUpdate, bool bonus)
    {
        totalScore += scoreUpdate;
        spendableScore += scoreUpdate;

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

    [Server]
    public void UpdateSpendableScore(int scoreUpdate)
    {
        spendableScore += scoreUpdate;
    }

    public int GetSpendableScore()
    {
        return spendableScore;
    }
}

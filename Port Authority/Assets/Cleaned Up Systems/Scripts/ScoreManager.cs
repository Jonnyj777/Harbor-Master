using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;
    [SyncVar] private int totalScore = 0;
    [SyncVar(hook = nameof(OnScoreChanged))] private int spendableScore = 0;
    public TextMeshProUGUI losePopUpText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI popUpText;
    public bool hasBonus = false;

    private void Start()
    {
        //UpdateScoreEntry();
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

        //UpdateScoreEntry();

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

    [Server]
    public void UpdateSpendableScore(int scoreUpdate)
    {
        spendableScore += scoreUpdate;

        //UpdateScoreEntry();
    }

    public int GetSpendableScore()
    {
        return spendableScore;
    }
}
/*
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using static UnityEngine.PlayerLoop.EarlyUpdate;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;
    [SyncVar(hook = nameof(OnScoreChanged))] private int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI popUpText;
    public bool hasBonus = false;

    private void Start()
    {
        scoreText.text = "$ 0";
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
        score += scoreUpdate;
        hasBonus = bonus;
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
}
*/


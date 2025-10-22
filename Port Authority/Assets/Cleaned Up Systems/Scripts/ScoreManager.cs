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

        StartCoroutine(ShowPopUp("+$ " + (newVal-oldVal), hasBonus));
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

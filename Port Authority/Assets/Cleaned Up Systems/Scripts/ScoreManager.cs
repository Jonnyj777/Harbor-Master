using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;
    [SyncVar] private int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI popUpText;

    private void Start()
    {
        scoreText.text = "$ 0";
    }

    void Awake()
    {
        Instance = this;
    }

    [Server]
    public void AddScore(int scoreUpdate, bool bonus)
    {
        score += scoreUpdate;
        scoreText.text = "$ " + score;

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
}

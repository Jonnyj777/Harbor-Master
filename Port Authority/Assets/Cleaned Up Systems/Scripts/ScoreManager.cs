using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI popUpText;
    public TextMeshProUGUI losePopUpText;

    private void Start()
    {
        scoreText.text = "$ 0";
    }

    void Awake()
    {
        Instance = this;
    }

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

    public void ShowLosePopUp()
    {
        losePopUpText.text = "Great Work!\r\n" +
                             "You safely delivered\r\n" +
                             "$" + score + "\r\n" +
                             "worth of goods\r\n" +
                             "this shift.";
        losePopUpText.gameObject.SetActive(true);
    }
}

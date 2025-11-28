using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Mirror;
using System;

public class LivesManagerN : NetworkBehaviour
{
    public static LivesManagerN Instance;
    [SyncVar(hook = nameof(UpdateLivesEntry))] private int lives = 6;
    public TextMeshProUGUI livesText;
    //public Button resetButton;
    public Button menuButton;


    void Start()
    {
        UpdateLivesEntry(lives, lives);
    }

    void Awake()
    {
        Instance = this;
    }

    [Server]
    public void AddLife()
    {
        int oldLives = lives;
        lives++;
        UpdateLivesEntry(oldLives, lives);
    }

    [Server]
    public void LoseLife()
    {
        int oldLives = lives;
        lives--;
        UpdateLivesEntry(oldLives, lives);

        if (lives < 1)
        {
            // End the game
            //Time.timeScale = 0f;

            ScoreManagerUI.Instance.ShowLosePopUp();
            ShowOptionsPopUp();
        }
    }

    public void ShowOptionsPopUp()
    {
        //resetButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }

    private void UpdateLivesEntry(int oldVal, int newVal)
    {
        livesText.text = "Lives: " + newVal;
    }
}

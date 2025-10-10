using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;
    private int lives = 6;
    public TextMeshProUGUI livesText;
    public Button resetButton;
    public Button menuButton;

    void Start()
    {
        livesText.text = "Lives: " + lives;
    }

    void Awake()
    {
        Instance = this;
    }

    public void LoseLife()
    {
        lives--;
        livesText.text = "Lives: " + lives;

        if (lives < 1)
        {
            // End the game
            Time.timeScale = 0f;

            ScoreManager.Instance.ShowLosePopUp();
            ShowOptionsPopUp();
        }
    }

    public void ShowOptionsPopUp()
    {
        resetButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }
}

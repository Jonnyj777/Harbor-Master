using TMPro;
using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;
    private int lives = 15;
    public TextMeshProUGUI livesText;

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
        }
    }
}

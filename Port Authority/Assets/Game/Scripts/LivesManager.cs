using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;
    private int lives = 1;
    public TextMeshProUGUI livesText;
    public Button resetButton;
    public Button menuButton;

    void Start()
    {
        UpdateLivesEntry();
    }

    void Awake()
    {
        Instance = this;
    }

    public void AddLife()
    {
        lives++;
        UpdateLivesEntry();
    }

    public void LoseLife()
    {
        lives--;
        UpdateLivesEntry();

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

    private void UpdateLivesEntry()
    {
        livesText.text = "Lives: " + lives;
    }
}

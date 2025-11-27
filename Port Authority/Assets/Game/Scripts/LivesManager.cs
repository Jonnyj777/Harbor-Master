using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;
    [SerializeField] private int startingLives = 6;
    public TextMeshProUGUI livesText;
    public Button resetButton;
    public Button menuButton;
    private LivesManagerLogic logic;

    void Awake()
    {
        Instance = this;
        logic = new LivesManagerLogic(startingLives);
    }

    void Start()
    {
        UpdateLivesEntry();
    }

    public void AddLife()
    {
        logic.AddLife();
        UpdateLivesEntry();
    }

    public void LoseLife()
    {
        bool outOfLives = logic.LoseLife();
        UpdateLivesEntry();

        if (outOfLives)
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
        livesText.text = "Lives: " + logic.Lives;
    }
}

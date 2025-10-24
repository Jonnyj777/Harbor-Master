using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCard : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text colorNameText;
    public TMP_Text initialText;
    public Image colorCircle;
    public Image outline;
    public Image pictureFrame;
    public Image profilePicture;
    public GameObject hostIcon;

    [Header("Optional Ready Button")]
    public Button readyButton;                
    public TMP_Text readyButtonText;         
    public Image readyButtonBackground;

    private bool isReady = false;
    private Color readyColor;
    private Color notReadyColor;

    private void Start()
    {
        ColorUtility.TryParseHtmlString("#16DA23", out readyColor); // green
        ColorUtility.TryParseHtmlString("#9D9D9D", out notReadyColor); // gray
    }

    public void SetPlayerInfo(string name, string colorName, Color color, bool isHost = false, bool showReadyButton = false, Sprite steamProfilePicture = null)
    {
        // set player text and color
        playerNameText.text = name;
        colorNameText.text = colorName;
        colorCircle.color = color;
        outline.color = color;


        // set profile picture, or colored circle with initial if no picture
        if (steamProfilePicture != null)
        {
            profilePicture.gameObject.SetActive(true);
            profilePicture.sprite = steamProfilePicture;
            initialText.gameObject.SetActive(false);
        }
        else
        {
            pictureFrame.color = color;
            if (initialText)
            {
                initialText.gameObject.SetActive(true);
                initialText.text = char.ToUpper(name[0]).ToString();
            }
        }

        // set host icon
        if (isHost)
        { 
            hostIcon.SetActive(true);
        }
        else
        {
            hostIcon.SetActive(false);
        }

        // toggle ready button
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(showReadyButton);
            if (showReadyButton)
            {
                UpdateReadyButton();
            }
        }
    }

    private void UpdateReadyButton()
    {
        if (isReady)
        {
            readyButtonText.text = "Ready";
            readyButtonBackground.color = readyColor;
        }
        else
        {
            readyButtonText.text = "Not Ready";
            readyButtonBackground.color = notReadyColor;
        }
    }

    public void ToggleReadyState()
    {
        isReady = !isReady;
        UpdateReadyButton();
    }

    public void SetReadyButtonVisible(bool visible)
    {
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(visible);
        }
    }

}

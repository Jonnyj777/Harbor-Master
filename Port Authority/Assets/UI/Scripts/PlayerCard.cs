using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

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

    public bool isReady = false;
    private Color readyColor;
    private Color notReadyColor;
    public SteamId id;

    public PlayerInfo playerInfo;


    private void Start()
    {
        ColorUtility.TryParseHtmlString("#16DA23", out readyColor); // green
        ColorUtility.TryParseHtmlString("#9D9D9D", out notReadyColor); // gray


        UpdateReadyButton(isReady);
        UpdateColorVisual("Blue");
    }

    public void SetPlayerInfo(string name, string colorName, Color color, SteamId memberId, bool isHost = false, Sprite steamProfilePicture = null)
    {
        // set player text and color
        playerNameText.text = name;

        if (colorNameText != null)
        {
            colorNameText.text = colorName;
        }

        if (colorCircle != null)
        {
            colorCircle.color = color;
        }

        outline.color = color;
        id = memberId;

        readyButton.gameObject.SetActive(true);
    }

    public void UpdateHost(bool isHost)
    {
        hostIcon.SetActive(isHost);
    }

    public void UpdateReadyButton(bool ready)
    {
        isReady = ready;

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
    
    public void SetDefaultReadyButton()
    {
        readyButtonText.text = "Not Ready";
        readyButtonBackground.color = notReadyColor;
    }

    public void UpdateColorVisual(string colorName)
    {
        UnityEngine.Color colorData;
        switch (colorName)
        {
            case "Orange":
                colorData = new UnityEngine.Color(255f / 255f, 119f / 255f, 0f / 255f);
                break;
            case "Blue":
                colorData = new UnityEngine.Color(14f / 255f, 165f / 255f, 233f / 255f);
                break;
            case "Pink":
                colorData = new UnityEngine.Color(255f / 255f, 31f / 255f, 139f / 255f);
                break;
            case "Purple":
                colorData = new UnityEngine.Color(182f / 255f, 27f / 255f, 243f / 255f);
                break;
            case "Red":
                colorData = new UnityEngine.Color(233f / 255f, 14f / 255f, 18f / 255f);
                break;
            case "Yellow":
                colorData = new UnityEngine.Color(255f / 255f, 217f / 255f, 0f / 255f);
                break;
            case "Green":
                colorData = new UnityEngine.Color(22f / 255f, 218f / 255f, 35f / 255f);
                break;
            default:
                colorData = new UnityEngine.Color(1.0f, 1.0f, 1.0f);
                break;
        }

        colorNameText.text = colorName;
        outline.color = colorData;
        colorCircle.color = colorData;

    }

    public void Connect(PlayerInfo info)
    {
        playerInfo = info;
        info.onValueChanged.AddListener(UpdateReadyButton);
        UpdateReadyButton(info.IsReady);
    }

    //public void ToggleReadyState()
    //{
    //    isReady = !isReady;
    //    playerInfo.IsReady = isReady;
    //    UpdateReadyButton();
    //}
}

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
    private readonly PlayerCardVisualLogic visualLogic = new PlayerCardVisualLogic();


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
        PlayerReadyState state = visualLogic.GetReadyState(ready);
        readyButtonText.text = state.Label;
        readyButtonBackground.color = state.IsReady ? readyColor : notReadyColor;
    }

    public void UpdateColorVisual(string colorName)
    {
        Color colorData = visualLogic.ResolveColor(colorName);

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

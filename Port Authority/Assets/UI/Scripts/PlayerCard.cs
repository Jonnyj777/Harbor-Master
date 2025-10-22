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
    public Image picture;

    [Header("Host Icon")]
    public GameObject hostIcon; // assign the crown icon in inspector

    /// <summary>
    /// Sets the info for the player card and optionally shows the host crown.
    /// </summary>
    public void SetPlayerInfo(string name, string colorName, Color color, bool isHost = false)
    {
        if (playerNameText) playerNameText.text = name;
        if (colorNameText) colorNameText.text = colorName;
        if (colorCircle) colorCircle.color = color;
        if (outline) outline.color = color;
        if (picture) picture.color = color;

        if (initialText && !string.IsNullOrEmpty(name))
        {
            initialText.text = char.ToUpper(name[0]).ToString();
        }

        // Show or hide the host crown
        if (hostIcon != null)
            hostIcon.SetActive(isHost);
    }
}

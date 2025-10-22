using System.Collections;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LobbyEntry : MonoBehaviour
{
    [Header("Background")]
    public Image background;
    public Sprite unselectedBackground;
    public Sprite selectedBackground;

    [Header("Lobby Info")]
    public TMP_Text lobbyNameText;
    public TMP_Text hostText;

    private UnityEngine.Color lobbyNameColor;
    private UnityEngine.Color hostTextColor;
    private UnityEngine.Color selectedTextColor;

    [Header("Player Count")]
    public Image countBackground;
    public TMP_Text countText;

    private UnityEngine.Color countColor;
    private UnityEngine.Color countSelectedColor;
    private UnityEngine.Color countFullColor;

    public bool isSelected = false;
    public bool isFull = false;

    void Start()
    {
        ColorUtility.TryParseHtmlString("#38BDF8", out countColor); // blue
        ColorUtility.TryParseHtmlString("#FFFFFF", out selectedTextColor); // white
        ColorUtility.TryParseHtmlString("#FFFFFF20", out countSelectedColor); // white transparent
        ColorUtility.TryParseHtmlString("#E90E12", out countFullColor); // red
        ColorUtility.TryParseHtmlString("#000000", out lobbyNameColor); // black
        ColorUtility.TryParseHtmlString("#9D9D9D", out hostTextColor); // gray

        SetUnselected();
    }

    public void SetSelected()
    {
        isSelected = true;
        background.sprite = selectedBackground;
        countBackground.color = isFull ? countFullColor : countSelectedColor;
        lobbyNameText.color = selectedTextColor;
        hostText.color = selectedTextColor;
    }


    public void SetUnselected()
    {
        isSelected = false;
        background.sprite = unselectedBackground;
        countBackground.color = isFull ? countFullColor : countColor;
        lobbyNameText.color = lobbyNameColor;
        hostText.color = hostTextColor;
    }

    public void SetFull(bool full = true)
    {
        isFull = full;
        countBackground.color = full ? countFullColor : countColor;
    }
}

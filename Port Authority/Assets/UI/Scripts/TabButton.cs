using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image backgroundImage; 
    public Image iconImage;       

    [Header("Sprites")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;

    [Header("Colors")]
    public Color activeIconColor = Color.white;
    public Color inactiveIconColor = Color.gray;

    private void Start()
    {
        ColorUtility.TryParseHtmlString("#9D9D9D", out inactiveIconColor); // gray
    }
    public void ToggleActive(bool isActive)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = isActive ? activeSprite : inactiveSprite;
        }

        if (iconImage != null)
        {
            iconImage.color = isActive ? activeIconColor : inactiveIconColor;
        }
    }
}

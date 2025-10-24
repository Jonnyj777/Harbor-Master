using UnityEngine;
using UnityEngine.UI;

public class ColorChoice : MonoBehaviour
{
    [Header("Color Info")]
    public string colorName;    
    public Image colorImage;  
    public GameObject selectedIcon;
    public Color color;

    private void Awake()
    {
        selectedIcon.SetActive(false);
        color = colorImage.color;
    }

    public void Select()
    {
        selectedIcon.SetActive(true);
    }

    public void Unselect()
    {
        selectedIcon.SetActive(false);
    }
}

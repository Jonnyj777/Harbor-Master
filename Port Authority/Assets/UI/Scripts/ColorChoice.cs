using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorChoice : MonoBehaviour
{
    [Header("Color Info")]
    public string colorName;    
    public Image colorImage;  
    public GameObject selectedIcon;
    public Color color;
    private bool isSelected;

    private void Awake()
    {
        color = colorImage.color;
        selectedIcon.SetActive(false);
    }

    private void OnEnable()
    {
        selectedIcon.SetActive(isSelected);
    }

    public void Select()
    {
        isSelected = true;
        selectedIcon.SetActive(true);
    }

    public void Unselect()
    {
        isSelected = false;
        selectedIcon.SetActive(false);
    }
}

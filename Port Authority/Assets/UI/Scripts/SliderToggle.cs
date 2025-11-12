using UnityEngine;
using UnityEngine.UI;

public class SliderToggle : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.value = 0;
    }
    public void Toggle()
    {
        slider.value = slider.value == 0 ? 1 : 0;
    }
}


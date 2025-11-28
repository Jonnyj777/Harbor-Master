using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class Spinner : MonoBehaviour
{
    [Header("Buttons")]
    public Button incrementButton;
    public Button decrementButton;

    [Header("Settings")]
    public int step = 1;
    public int minValue = 2;
    public int maxValue = 5;

    private TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();

        incrementButton.onClick.AddListener(Increment);
        decrementButton.onClick.AddListener(Decrement);

        SetValue(minValue);
    }

    public void Increment()
    {
        int value = GetCurrentValue();
        value += step;
        value = Mathf.Clamp(value, minValue, maxValue);
        SetValue(value);
    }

    public void Decrement()
    {
        int value = GetCurrentValue();
        value -= step;
        value = Mathf.Clamp(value, minValue, maxValue);
        SetValue(value);
    }

    private int GetCurrentValue()
    {
        if (int.TryParse(inputField.text, out int value))
        {
            return value;
        }
        return minValue;
    }

    private void SetValue(int value)
    {
        inputField.text = value.ToString();
    }
}

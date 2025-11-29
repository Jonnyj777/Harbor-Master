using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsData : MonoBehaviour
{
    [Header("Gameplay")]

    [Header("Audio")]
    public Slider masterVolumeSlider;
    public TMP_Text masterVolumeText;

    public Slider sfxVolumeSlider;
    public TMP_Text sfxVolumeText;

    public Slider musicVolumeSlider;
    public TMP_Text musicVolumeText;

    [Header("Data")]
    public int masterVolume = 100;
    public int sfxVolume = 100;
    public int musicVolume = 100;

    public Transform colorChoicesContainer;
    public List<ColorChoice> colorChoices;

    public ColorChoice selectedColorChoice;

    private void Start()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        colorChoices = new List<ColorChoice>(colorChoicesContainer.GetComponentsInChildren<ColorChoice>());

        for (int i = 0; i < colorChoices.Count; i++)
        {
            Button btn = colorChoices[i].GetComponent<Button>();

            ColorChoice choice = colorChoices[i];

            string name = colorChoices[i].colorName;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnColorClicked(choice));
        }

        // initialize the display once
        InitializeValues();
    }

    private void OnColorClicked(ColorChoice color)
    {
        if (selectedColorChoice != null)
        {
            selectedColorChoice.Unselect();
        }

        // select new entry and display lobby
        selectedColorChoice = color;
        selectedColorChoice.Select();

        LineColorManager.lineColorName = selectedColorChoice.colorName;
    }

    private void InitializeValues()
    {
        // load saved volumes from PlayerPrefs (default to 1.0 if not found)
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSfx = PlayerPrefs.GetFloat("SfxVolume", 1f);

        masterVolume = Mathf.RoundToInt(savedMaster * 100);
        musicVolume = Mathf.RoundToInt(savedMusic * 100);
        sfxVolume = Mathf.RoundToInt(savedSfx * 100);

        masterVolumeSlider.value = savedMaster;
        musicVolumeSlider.value = savedMusic;
        sfxVolumeSlider.value = savedSfx;

        masterVolumeText.text = $"{masterVolume}%";
        musicVolumeText.text = $"{musicVolume}%";
        sfxVolumeText.text = $"{sfxVolume}%";

        OnColorClicked(colorChoices[0]);
    }

    // Gameplay

    // Audio
    void OnMasterVolumeChanged(float value)
    {
        masterVolume = Mathf.RoundToInt(value * 100);
        masterVolumeText.text = $"{masterVolume}%";
    }

    void OnMusicVolumeChanged(float value)
    {
        musicVolume = Mathf.RoundToInt(value * 100);
        musicVolumeText.text = $"{musicVolume}%";
    }

    void OnSfxVolumeChanged(float value)
    {
        sfxVolume = Mathf.RoundToInt(value * 100);
        sfxVolumeText.text = $"{sfxVolume}%";
    }
}

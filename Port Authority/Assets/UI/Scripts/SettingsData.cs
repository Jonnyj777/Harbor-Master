using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsData : MonoBehaviour
{
    [Header("Gameplay")]
    public Slider tutorialToggle;
    public TMP_Dropdown difficultyDropdown;

    [Header("Graphics")]
    public Slider fullscreenToggle;
    public TMP_Dropdown qualityDropdown;

    [Header("Audio")]
    public Slider masterVolumeSlider;
    public TMP_Text masterVolumeText;

    public Slider sfxVolumeSlider;
    public TMP_Text sfxVolumeText;

    public Slider musicVolumeSlider;
    public TMP_Text musicVolumeText;

    [Header("Data")]
    public bool tutorialEnabled = true;
    public string difficulty = "";
    public bool fullscreen = false;
    public string quality = "";
    public int masterVolume = 100;
    public int sfxVolume = 100;
    public int musicVolume = 100;

    private void Start()
    {
        tutorialToggle.onValueChanged.AddListener(OnTutorialChanged);
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);

        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        

        // Initialize the display once
        InitializeValues();
    }

    private void InitializeValues()
    {
        tutorialToggle.value = tutorialEnabled ? 1 : 0;
        fullscreenToggle.value = fullscreen ? 1 : 0;

        difficultyDropdown.value = 1;
        qualityDropdown.value = 1;

        masterVolumeSlider.value = masterVolume / 100f;
        musicVolumeSlider.value = musicVolume / 100f;
        sfxVolumeSlider.value = sfxVolume / 100f;

        masterVolumeText.text = $"{masterVolume}%";
        musicVolumeText.text = $"{musicVolume}%";
        sfxVolumeText.text = $"{sfxVolume}%";
    }

    // Gameplay
    void OnTutorialChanged(float value)
    {
        tutorialEnabled = value >= 0.5f;
    }

    void OnDifficultyChanged(int index)
    {
        difficulty = difficultyDropdown.options[index].text;
    }

    // Graphics
    void OnFullscreenChanged(float value)
    {
        fullscreen = value >= 0.5f;
    }

    void OnQualityChanged(int index)
    {
        quality = qualityDropdown.options[index].text;
    }

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

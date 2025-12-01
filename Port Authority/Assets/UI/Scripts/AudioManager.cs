using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("UI Sounds")]
    public AudioClip buttonHover;
    public AudioClip buttonClick;
    public AudioClip buttonBack;
    public AudioClip buttonCreate;
    public AudioClip buttonCancel;
    public AudioClip buttonJoin;

    [Header("Gameplay Sounds")]
    public AudioClip gameMusic;
    public AudioClip boatEntrance;
    public AudioClip boatDelivery;
    public AudioClip boatCollision;
    public AudioClip truckDelivery;
    public AudioClip truckCollision;
    public AudioClip landObstacleSpawn;
    public AudioClip landObstacleCleanup;

    [Header("Ambient Sound")]
    public AudioClip ambientWaves;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioSource ambientSource;

    [Header("Game Settings")]
    public bool sfxEnabled = true;
    public bool musicEnabled = true;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Header("UI References")]
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    public TMP_Text masterPercentage;
    public TMP_Text sfxPercentage;
    public TMP_Text musicPercentage;

    public Toggle sfxToggle;
    public Toggle musicToggle;

    private void Awake()
    {
        // ensure one AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // set up audio sources
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.playOnAwake = false;
        ambientSource.loop = true;

        // restores volume of player choice (persistence)
        LoadVolumeSettings();

        // if in scene, initialize sliders
        InitializeSliders();

        // apply volumes
        ApplyVolume();

        // if assigned, immediately start ambient waves audio file
        if (ambientWaves != null)
        {
            PlayAmbientWaves();
        }

        if (musicToggle != null)
        {
            musicEnabled = musicToggle.isOn;
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }
        else
        {
            Debug.LogWarning("AudioManager: Music Toggle not assigned in Inspector");
        }
    }

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "LevelWithSFX" && musicEnabled)
        {
            PlayMusic();
        }
    }
    
    public void PlayMusic()
    {
        if (!musicEnabled || musicSource == null || gameMusic == null)
        {
            return;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.clip = gameMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void InitializeSliders()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            //float prevValue = masterSlider.value;
            masterSlider.SetValueWithoutNotify(masterVolume);
            masterSlider.onValueChanged.AddListener(val =>
            {
                SetMasterVolume(val);
                UpdatePercentage(masterPercentage, val);
            });
            UpdatePercentage(masterPercentage, masterVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.SetValueWithoutNotify(sfxVolume);
            sfxSlider.onValueChanged.AddListener(val =>
            {
                SetSfxVolume(val);
                UpdatePercentage(sfxPercentage, val);
            });
            UpdatePercentage(sfxPercentage, sfxVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.SetValueWithoutNotify(musicVolume);
            musicSlider.onValueChanged.AddListener(val =>
            {
                SetMusicVolume(val);
                UpdatePercentage(musicPercentage, val);
            });
            UpdatePercentage(musicPercentage, musicVolume);
        }
    }

    private void UpdatePercentage(TMP_Text text, float value)
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    public void PlayAmbientWaves()
    {
        if (ambientSource.isPlaying)
        {
            return;
        }

        ambientSource.clip = ambientWaves;
        ambientSource.volume = masterVolume * sfxVolume;
        ambientSource.Play();
    }

    public void StopAmbientWaves()
    {
        if (ambientSource.isPlaying)
        {
            ambientSource.Stop();
        }
    }

    private void ApplyVolume()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }

        if (musicSource != null)
        {
            musicSource.volume = masterVolume * (musicVolume * 0.15f);
        }

        if (ambientSource != null)
        {
            ambientSource.volume = masterVolume * sfxVolume;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        ApplyVolume();
        SaveVolumeSettings();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        ApplyVolume();
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        ApplyVolume();
        SaveVolumeSettings();
    }

    // BUTTON SFX
    public void PlayHover()
    {
        if (buttonHover != null)
        {
            sfxSource.PlayOneShot(buttonHover, sfxSource.volume);
        }
    }

    public void PlayClick()
    {
        if (buttonClick != null)
        {
            sfxSource.PlayOneShot(buttonClick, sfxSource.volume);
        }
    }

    public void BackButton()
    {
        if (buttonBack != null)
        {
            sfxSource.PlayOneShot(buttonBack, sfxSource.volume);
        }
    }

    public void CreateButton()
    {
        if (buttonCreate != null)
        {
            sfxSource.PlayOneShot(buttonCreate, sfxSource.volume);
        }
    }

    public void CancelButton()
    {
        if (buttonCancel != null)
        {
            sfxSource.PlayOneShot(buttonCancel, sfxSource.volume);
        }
    }

    public void JoinButton()
    {
        if (buttonJoin != null)
        {
            sfxSource.PlayOneShot(buttonJoin, sfxSource.volume);
        }
    }

    // GAMEPLAY SFX
    public void PlayBoatEntrance()
    {
        if (!sfxEnabled || boatEntrance == null)
        {
            return;
        }
        else
        {
            sfxSource.PlayOneShot(boatEntrance);
        }
    }

    public void PlayBoatDelivery()
    {
        // DM-CGS-24
        if (!sfxEnabled || boatDelivery == null)
        {
            return;
        }
        else
        {
            sfxSource.PlayOneShot(boatDelivery);
        }
    }

    public void PlayBoatCollision()
    {
        if (!sfxEnabled || boatCollision == null)
        {
            return;
        }
        else
        {
            sfxSource.PlayOneShot(boatCollision);
        }
    }

    public void PlayTruckDelivery()
    {
        if (!sfxEnabled || truckDelivery == null)
        {
            return;
        }
        else
        {
            sfxSource.PlayOneShot(truckDelivery);
        }
    }

    public void PlayTruckCollision()
    {
        if (!sfxEnabled || truckCollision == null)
        {
            return;
        }
        else
        {
            sfxSource.PlayOneShot(truckCollision);
        }
    }

    public void PlayLandObstacleSpawn()
    {
        if (!sfxEnabled || landObstacleSpawn == null) return;
        sfxSource.PlayOneShot(landObstacleSpawn, sfxSource.volume);
    }

    public void PlayLandObstacleCleanup()
    {
        if (!sfxEnabled || landObstacleCleanup == null) return;
        sfxSource.PlayOneShot(landObstacleCleanup, sfxSource.volume);
    }

    public void ToggleSFX(bool enabled)
    {
        sfxEnabled = enabled;
        //Debug.Log("SFX Enabled: " + sfxEnabled);
    }

    // SAVING VOLUME STATES
    // function to save the player's volume states
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    // load player's choice of volume settings if tweaked from original
    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume");
        }

        if (PlayerPrefs.HasKey("SfxVolume"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SfxVolume");
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.15f);
        }
    }

    public void ToggleMusic(bool enabled)
    {
        musicEnabled = enabled;
        if (musicEnabled)
        {
            PlayMusic();
        }
        else
        {
            StopMusic();
        }
    }
}

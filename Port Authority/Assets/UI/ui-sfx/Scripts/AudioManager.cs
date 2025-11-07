using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("UI Sounds")]
    public AudioClip gameMusic;
    public AudioClip buttonHover;
    public AudioClip buttonClick;
    public AudioClip boatEntrance;
    public AudioClip boatDelivery;
    public AudioClip boatCollision;
    public AudioClip truckDelivery;
    public AudioClip truckCollision;

    private AudioSource audioSource;

    [Header("Settings")]
    public bool sfxEnabled = true;
    public bool musicEnabled = true;

    [Header("UI References")]
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

        if (!TryGetComponent<AudioSource>(out audioSource))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        if (sfxToggle != null)
        {
            // read initial state
            sfxEnabled = sfxToggle.isOn;

            // hook up toggle callback
            sfxToggle.onValueChanged.AddListener(ToggleSFX);
        }
        else
        {
            // DEBUG
            //Debug.LogWarning("AudioManager: SFX Toggle not assigned in Inspector");
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
        if (!musicEnabled || audioSource == null || gameMusic == null)
        {
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.clip = gameMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PlayHover()
    {
        if (!sfxEnabled || buttonHover == null)
        { 
            return; 
        }
        else
        {
            audioSource.PlayOneShot(buttonHover);
        }
    }

    public void PlayClick()
    {
        if (!sfxEnabled || buttonClick == null)
        {
            return;
        }
        else
        {
            audioSource.PlayOneShot(buttonClick);
        }
    }

    public void PlayBoatEntrance()
    {
        if (!sfxEnabled || boatEntrance == null)
        {
            return;
        }
        else
        {
            audioSource.PlayOneShot(boatEntrance);
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
            audioSource.PlayOneShot(boatDelivery);
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
            audioSource.PlayOneShot(boatCollision);
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
            audioSource.PlayOneShot(truckDelivery);
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
            audioSource.PlayOneShot(truckCollision);
        }
    }

    public void ToggleSFX(bool enabled)
    {
        sfxEnabled = enabled;
        //Debug.Log("SFX Enabled: " + sfxEnabled);
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

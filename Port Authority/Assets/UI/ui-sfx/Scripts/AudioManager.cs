using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("UI Sounds")]
    public AudioClip buttonHover;
    public AudioClip buttonClick;

    private AudioSource audioSource;

    [Header("Settings")]
    public bool sfxEnabled = true;

    [Header("UI References")]
    public Toggle sfxToggle;

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

    public void ToggleSFX(bool enabled)
    {
        sfxEnabled = enabled;
        //Debug.Log("SFX Enabled: " + sfxEnabled);
    }
}

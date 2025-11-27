using UnityEngine;

/// <summary>
/// Pure container for audio preferences so the AudioManager can focus on Unity components.
/// </summary>
public sealed class AudioSettingsLogic
{
    public float MasterVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public bool SfxEnabled { get; private set; }
    public bool MusicEnabled { get; private set; }

    public AudioSettingsLogic(float masterVolume, float sfxVolume, float musicVolume, bool sfxEnabled, bool musicEnabled)
    {
        MasterVolume = Mathf.Clamp01(masterVolume);
        SfxVolume = Mathf.Clamp01(sfxVolume);
        MusicVolume = Mathf.Clamp01(musicVolume);
        SfxEnabled = sfxEnabled;
        MusicEnabled = musicEnabled;
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
    }

    public void SetSfxEnabled(bool enabled)
    {
        SfxEnabled = enabled;
    }

    public void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
    }

    public float GetEffectiveSfxVolume()
    {
        return SfxEnabled ? MasterVolume * SfxVolume : 0f;
    }

    public float GetEffectiveMusicVolume()
    {
        return MusicEnabled ? MasterVolume * MusicVolume : 0f;
    }
}

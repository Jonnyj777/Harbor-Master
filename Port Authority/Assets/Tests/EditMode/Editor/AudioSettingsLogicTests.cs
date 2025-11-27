using NUnit.Framework;

public class AudioSettingsLogicTests
{
    [Test]
    public void GetEffectiveSfxVolume_ZeroWhenDisabled()
    {
        var logic = new AudioSettingsLogic(1f, 0.5f, 0.5f, sfxEnabled: false, musicEnabled: true);

        Assert.AreEqual(0f, logic.GetEffectiveSfxVolume());
    }

    [Test]
    public void ToggleMusicAffectsEffectiveVolume()
    {
        var logic = new AudioSettingsLogic(1f, 0.5f, 0.5f, sfxEnabled: true, musicEnabled: false);

        logic.SetMusicEnabled(true);

        Assert.Greater(logic.GetEffectiveMusicVolume(), 0f);
    }
}

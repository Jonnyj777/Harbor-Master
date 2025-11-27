using NUnit.Framework;
using UnityEngine;

public class PlayerInfoTests
{
    [Test]
    public void SettingReadyFlag_NotifiesListeners()
    {
        var info = new PlayerInfo();
        bool invoked = false;
        info.onValueChanged.AddListener(value => invoked = value);

        info.IsReady = true;

        Assert.IsTrue(invoked);
    }
}

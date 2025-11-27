using NUnit.Framework;

public class SimpleVehicleSpawnScriptTests
{
    [Test]
    public void Scheduler_FiresAfterAccumulatedTime()
    {
        var random = new TestRandomProvider();
        random.EnqueueFloat(0.5f);
        var scheduler = new SpawnSchedulerLogic(2f, 10f, random);

        bool spawnTriggered = false;
        spawnTriggered |= scheduler.Tick(0.2f);
        spawnTriggered |= scheduler.Tick(0.2f);
        spawnTriggered |= scheduler.Tick(0.2f);

        Assert.IsTrue(spawnTriggered);
    }
}

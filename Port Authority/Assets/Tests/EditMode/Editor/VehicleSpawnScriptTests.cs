using NUnit.Framework;

public class VehicleSpawnScriptTests
{
    [Test]
    public void Scheduler_LowersSpawnRateTowardOne()
    {
        var random = new TestRandomProvider();
        random.EnqueueFloat(1f);
        var scheduler = new SpawnSchedulerLogic(3f, 0.5f, random);

        scheduler.Tick(0.5f);
        scheduler.Tick(0.5f);
        scheduler.Tick(0.5f);

        Assert.LessOrEqual(scheduler.CurrentSpawnRate, 2f);
        Assert.GreaterOrEqual(scheduler.CurrentSpawnRate, 1f);
    }
}

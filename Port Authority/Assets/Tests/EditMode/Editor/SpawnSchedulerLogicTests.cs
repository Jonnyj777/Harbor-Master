using NUnit.Framework;

public class SpawnSchedulerLogicTests
{
    [Test]
    public void Tick_ReturnsTrueWhenIntervalReached()
    {
        var random = new TestRandomProvider();
        random.EnqueueFloat(1f);
        var logic = new SpawnSchedulerLogic(5f, 10f, random);

        bool shouldSpawn = logic.Tick(1.5f);

        Assert.IsTrue(shouldSpawn);
    }

    [Test]
    public void Tick_DecreasesSpawnRateOverTime()
    {
        var random = new TestRandomProvider();
        random.EnqueueFloat(1f);
        var logic = new SpawnSchedulerLogic(5f, 1f, random);

        logic.Tick(1f);

        Assert.Less(logic.CurrentSpawnRate, 5f);
    }
}

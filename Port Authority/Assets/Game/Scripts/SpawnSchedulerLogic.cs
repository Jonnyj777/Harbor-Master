using UnityEngine;

/// <summary>
/// Controls spawn intervals and difficulty scaling in a pure C# form so tests can drive the timing.
/// </summary>
public sealed class SpawnSchedulerLogic
{
    private readonly IRandomProvider randomProvider;
    private readonly float difficultyIncreaseRate;
    private float spawnRate;
    private float minSpawnRate;
    private float currentSpawnInterval;
    private float spawnTimer;
    private float gameTimer;

    public SpawnSchedulerLogic(float spawnRate, float difficultyIncreaseRate, IRandomProvider randomProvider)
    {
        this.randomProvider = randomProvider;
        this.difficultyIncreaseRate = difficultyIncreaseRate;
        this.spawnRate = Mathf.Max(1f, spawnRate);
        minSpawnRate = Mathf.Max(1f, this.spawnRate - 2f);
        currentSpawnInterval = this.randomProvider.Range(minSpawnRate, this.spawnRate);
    }

    /// <summary>
    /// Advances the internal timers. Returns true whenever a spawn should occur this frame.
    /// </summary>
    public bool Tick(float deltaTime)
    {
        spawnTimer += deltaTime;
        gameTimer += deltaTime;

        bool shouldSpawn = false;

        if (spawnTimer >= currentSpawnInterval)
        {
            shouldSpawn = true;
            spawnTimer = 0f;
            currentSpawnInterval = randomProvider.Range(minSpawnRate, spawnRate);
        }

        if (gameTimer >= difficultyIncreaseRate)
        {
            gameTimer = 0f;
            if (minSpawnRate > 1f)
            {
                minSpawnRate -= 1f;
            }

            if (spawnRate > 1f)
            {
                spawnRate -= 1f;
            }
        }

        return shouldSpawn;
    }

    public float CurrentSpawnRate => spawnRate;
    public float CurrentMinSpawnRate => minSpawnRate;
}

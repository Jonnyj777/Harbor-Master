/// <summary>
/// Tracks the player's lives independently of Unity specific behaviour.
/// </summary>
public sealed class LivesManagerLogic
{
    public int Lives { get; private set; }

    public LivesManagerLogic(int initialLives)
    {
        Lives = initialLives < 0 ? 0 : initialLives;
    }

    public void AddLife()
    {
        Lives++;
    }

    /// <summary>
    /// Returns true if the loss caused the lives to drop to zero.
    /// </summary>
    public bool LoseLife()
    {
        if (Lives > 0)
        {
            Lives--;
        }

        return Lives <= 0;
    }
}

/// <summary>
/// Tracks total and spendable score without MonoBehaviour dependencies.
/// </summary>
public sealed class ScoreManagerLogic
{
    public int TotalScore { get; private set; }
    public int SpendableScore { get; private set; }

    public ScoreManagerLogic(int initialTotal = 0, int initialSpendable = 0)
    {
        TotalScore = initialTotal;
        SpendableScore = initialSpendable;
    }

    public void AddScore(int amount)
    {
        TotalScore += amount;
        SpendableScore += amount;
    }

    public void AdjustSpendable(int delta)
    {
        SpendableScore += delta;
    }
}

/// <summary>
/// Provides deterministic UI state for one-time purchase cards so we can unit test without Unity UI.
/// </summary>
public sealed class PurchaseCardLogic
{
    public PurchaseCardState BuildState(bool isPurchased)
    {
        return new PurchaseCardState
        {
            ButtonLabel = isPurchased ? "Owned" : "Buy",
            IsInteractable = !isPurchased,
            UseBoughtVisual = isPurchased
        };
    }
}

public struct PurchaseCardState
{
    public string ButtonLabel;
    public bool IsInteractable;
    public bool UseBoughtVisual;
}

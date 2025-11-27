/// <summary>
/// Extracts the simple truck decision making so edit-mode tests can validate corner cases.
/// </summary>
public sealed class TruckStateLogic
{
    public bool ShouldBounceBack(bool isOverWater, bool isBouncingBack)
    {
        return isOverWater && !isBouncingBack;
    }

    public bool CanStartPickup(int cargoCount, int capacity, bool isPickingUp, int portCargoCount)
    {
        if (isPickingUp)
        {
            return false;
        }

        if (cargoCount >= capacity)
        {
            return false;
        }

        return portCargoCount > 0;
    }

    public bool CanApplyMudEffect(bool mudEffected, bool isCrashed)
    {
        return !mudEffected && !isCrashed;
    }
}

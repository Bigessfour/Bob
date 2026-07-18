/// <summary>
/// Shared reward values for UI popups and BobAgent scoring.
/// </summary>
public static class ArcAcademyRewards
{
    public const float MadeBasket = 7.0f;      // Raised significantly for strong contrast vs dense shaping
    public const float SwishBonus = 1.0f;      // More rewarding for clean swishes
    public const float MadeWithSwish = MadeBasket + SwishBonus;

    /// <summary>Displayed basketball score — one point per made free throw (separate from RL reward).</summary>
    public const int BasketballPointValue = 1;
}

/// <summary>
/// Shared reward values for UI popups and BobAgent scoring.
/// </summary>
public static class ArcAcademyRewards
{
    /// <summary>bob-v4.1 Tier 1.5 — raised so makes dominate near-miss shaping.</summary>
    public const float MadeBasket = 7.0f;

    public const float SwishBonus = 0.75f;
    public const float MadeWithSwish = MadeBasket + SwishBonus;

    /// <summary>Displayed basketball score — one point per made free throw (separate from RL reward).</summary>
    public const int BasketballPointValue = 1;
}

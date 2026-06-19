/// <summary>
/// Shared reward values for UI popups and BobAgent scoring.
/// </summary>
public static class ArcAcademyRewards
{
    public const float MadeBasket = 2.0f;
    public const float SwishBonus = 0.5f;
    public const float MadeWithSwish = MadeBasket + SwishBonus;

    /// <summary>Displayed basketball score — one point per made free throw (separate from RL reward).</summary>
    public const int BasketballPointValue = 1;
}

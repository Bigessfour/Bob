/// <summary>
/// Shared reward values for UI popups and BobAgent scoring.
///
/// Free-throw objective: launch straight with enough upward arch and force that the
/// ball arrives at the hoop and falls completely through top → bottom (toward the floor).
/// Any path that fully passes through counts as one point (swish, rim-in, or bank).
/// Anything that does not fully go through = no point.
///
/// Early curriculum: a smaller reward for hitting the backboard shooter's square
/// (high-arc bank target) reinforces useful aim without equaling a make.
/// </summary>
public static class ArcAcademyRewards
{
    /// <summary>
    /// Sparse make reward when <see cref="HoopScoreZone"/> confirms a descending
    /// pass through the cylinder. Identical for swish / rim-in / bank.
    /// Must dominate dense launch/arc shaping and miss penalties (Tier 1.5).
    /// </summary>
    public const float MadeBasket = 8.0f;

    /// <summary>
    /// Hit inside the backboard shooter's square (orange target box). Curriculum only —
    /// must stay well below <see cref="MadeBasket"/> so PPO still prefers real makes.
    /// </summary>
    public const float BackboardSquareHit = 1.0f;

    /// <summary>
    /// Cosmetic only (speech / VFX). Not applied to RL — a make is a make.
    /// </summary>
    public const float SwishBonus = 0f;

    /// <summary>Same as <see cref="MadeBasket"/> — kept for HUD string compatibility.</summary>
    public const float MadeWithSwish = MadeBasket + SwishBonus;

    /// <summary>
    /// Unused for RL. Rim graze is not punished: rim-in is a valid make;
    /// rim-out is already a miss (no make reward + existing miss shaping).
    /// </summary>
    public const float RimContactPenalty = 0f;

    /// <summary>Displayed basketball score — one point per made free throw (separate from RL reward).</summary>
    public const int BasketballPointValue = 1;
}

using UnityEngine;

/// <summary>
/// Shared bounce materials for the active hoop rim, backboard, and net.
/// </summary>
public static class HoopPhysicsMaterials
{
    private static PhysicsMaterial s_rim;
    private static PhysicsMaterial s_backboard;
    private static PhysicsMaterial s_net;

    public static PhysicsMaterial Rim
    {
        get
        {
            if (s_rim == null)
            {
                s_rim = Create("HoopRim", 0.55f, 0.18f);
            }

            return s_rim;
        }
    }

    public static PhysicsMaterial Backboard
    {
        get
        {
            if (s_backboard == null)
            {
                s_backboard = Create("HoopBackboard", 0.32f, 0.28f);
            }

            return s_backboard;
        }
    }

    public static PhysicsMaterial NetStrand
    {
        get
        {
            if (s_net == null)
            {
                s_net = Create("HoopNet", 0.18f, 0.35f);
            }

            return s_net;
        }
    }

    /// <summary>Legacy alias used by older scene code paths.</summary>
    public static PhysicsMaterial HoopBounce => Rim;

    private static PhysicsMaterial Create(string name, float bounciness, float friction)
    {
        return new PhysicsMaterial(name)
        {
            bounciness = bounciness,
            dynamicFriction = friction,
            staticFriction = friction,
            bounceCombine = PhysicsMaterialCombine.Maximum,
            frictionCombine = PhysicsMaterialCombine.Average,
        };
    }
}

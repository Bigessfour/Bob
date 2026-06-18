using UnityEngine;

/// <summary>
/// Shared bounce materials for the active hoop rim and backboard.
/// </summary>
public static class HoopPhysicsMaterials
{
    private static PhysicsMaterial s_hoopBounce;

    public static PhysicsMaterial HoopBounce
    {
        get
        {
            if (s_hoopBounce == null)
            {
                s_hoopBounce = new PhysicsMaterial("HoopBounce")
                {
                    bounciness = 0.45f,
                    dynamicFriction = 0.2f,
                    staticFriction = 0.2f,
                    bounceCombine = PhysicsMaterialCombine.Maximum,
                    frictionCombine = PhysicsMaterialCombine.Average,
                };
            }

            return s_hoopBounce;
        }
    }

    public static PhysicsMaterial NetStrand => HoopBounce;
}

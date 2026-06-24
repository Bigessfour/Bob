/// <summary>
/// Play-session flags shared between runtime training monitor and editor Play-mode guard.
/// </summary>
public static class BobTrainingSessionFlags
{
    public static bool WasTrainingConnectedThisPlaySession { get; private set; }

    public static void MarkTrainerConnected()
    {
        WasTrainingConnectedThisPlaySession = true;
    }

    public static void ResetPlaySession()
    {
        WasTrainingConnectedThisPlaySession = false;
    }
}

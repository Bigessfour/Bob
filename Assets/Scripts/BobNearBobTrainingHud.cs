using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Large floating board near Bob — primary readable Episodes / Score / Success / last shot.
/// Wall console (<see cref="BobWallTrainingHud"/>) keeps graph + RL detail.
/// </summary>
public class BobNearBobTrainingHud : MonoBehaviour
{
    public const string RootName = "NearBobTrainingHud";

    public static BobNearBobTrainingHud Instance { get; private set; }

    [SerializeField] private Text titleText;
    [SerializeField] private Text episodesText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text successText;
    [SerializeField] private Text lastShotText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text launchText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        BindMissingReferences();
        EnsureReadableStyles();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        var stats = BobTrainingStats.Instance;
        if (stats == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = "BOB · LIVE";
        }

        if (episodesText != null)
        {
            episodesText.text = $"{BobScoreboardDisplay.EpisodesLabel}  {stats.TotalIterations}";
        }

        if (scoreText != null)
        {
            scoreText.text = $"{BobScoreboardDisplay.ScoreLabel}  {stats.BasketballPoints}";
        }

        if (successText != null)
        {
            successText.text =
                $"{BobScoreboardDisplay.SuccessLabel}  {stats.SessionSuccessRate:P0}   ·   " +
                $"{BobScoreboardDisplay.RollingLabel} {stats.RollingSuccessRate:P0}";
        }

        if (lastShotText != null)
        {
            lastShotText.text =
                $"{BobScoreboardDisplay.LastShotRlLabel}  {stats.LastEpisodeNetReward:+0.0;-0.0}   ·   " +
                $"{BobScoreboardDisplay.ArcLabel} {stats.LastEpisodePeakArcQuality:P0}   ·   {stats.LastShotEndReason}";
        }

        var monitor = BobTrainingConnectionMonitor.Instance;
        if (statusText != null)
        {
            statusText.text = monitor != null ? monitor.StatusLabel : "Play mode";
            statusText.color = BobScoreboardDisplay.StatusColor(
                monitor != null && monitor.IsTrainingConnected);
        }

        if (launchText != null)
        {
            Vector3 a = stats.LastLaunchActions;
            Vector3 f = stats.LastLaunchImpulse;
            launchText.text =
                $"Launch a=({a.x:+0.00;-0.00},{a.y:+0.00;-0.00},{a.z:+0.00;-0.00})  " +
                $"F=({f.x:+0.0;-0.0},{f.y:+0.0;-0.0},{f.z:+0.0;-0.0})  toward {stats.LastTowardHoopDot:+0.00;-0.00}";
        }
    }

    private void BindMissingReferences()
    {
        var panel = transform.Find("Canvas/Panel");
        if (panel == null)
        {
            return;
        }

        titleText ??= panel.Find("TitleText")?.GetComponent<Text>();
        episodesText ??= panel.Find("EpisodesText")?.GetComponent<Text>();
        scoreText ??= panel.Find("ScoreText")?.GetComponent<Text>();
        successText ??= panel.Find("SuccessText")?.GetComponent<Text>();
        lastShotText ??= panel.Find("LastShotText")?.GetComponent<Text>();
        statusText ??= panel.Find("StatusText")?.GetComponent<Text>();
        launchText ??= panel.Find("LaunchText")?.GetComponent<Text>();
    }

    private void EnsureReadableStyles()
    {
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            titleText, BobScoreboardDisplay.FloatTitleFontSize, BobScoreboardDisplay.BodyColor, bold: true);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            episodesText, BobScoreboardDisplay.FloatHeroFontSize, BobScoreboardDisplay.HeadlineColor);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            scoreText, BobScoreboardDisplay.FloatScoreFontSize, BobScoreboardDisplay.ScoreAccentColor);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            successText, BobScoreboardDisplay.FloatSuccessFontSize, BobScoreboardDisplay.HeadlineColor);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            lastShotText, BobScoreboardDisplay.FloatDetailFontSize, BobScoreboardDisplay.BodyColor, bold: false);
        if (statusText != null)
        {
            BobScoreboardDisplay.ApplyFloatHeroTextStyle(
                statusText, BobScoreboardDisplay.FloatDetailFontSize,
                BobScoreboardDisplay.StatusDisconnectedColor, bold: false);
        }

        if (launchText != null)
        {
            BobScoreboardDisplay.ApplyFloatHeroTextStyle(
                launchText, BobScoreboardDisplay.FloatDetailFontSize, BobScoreboardDisplay.MutedDetailColor, bold: false);
        }
    }
}

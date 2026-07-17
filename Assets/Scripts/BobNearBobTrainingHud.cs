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
                $"{BobScoreboardDisplay.SuccessLabel}  {stats.SessionSuccessRate:P0}   ·   Rolling {stats.RollingSuccessRate:P0}";
        }

        if (lastShotText != null)
        {
            lastShotText.text =
                $"Last  {stats.LastEpisodeNetReward:+0.0;-0.0} RL   ·   Arc {stats.LastEpisodePeakArcQuality:P0}";
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
    }

    private void EnsureReadableStyles()
    {
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            titleText, BobScoreboardDisplay.FloatTitleFontSize, new Color(0.92f, 0.94f, 1f), bold: true);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            episodesText, BobScoreboardDisplay.FloatHeroFontSize, BobScoreboardDisplay.HeadlineColor);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            scoreText, BobScoreboardDisplay.FloatScoreFontSize, BobScoreboardDisplay.ScoreAccentColor);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            successText, BobScoreboardDisplay.FloatSuccessFontSize, BobScoreboardDisplay.HeadlineColor);
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(
            lastShotText, BobScoreboardDisplay.FloatDetailFontSize, BobScoreboardDisplay.BodyColor, bold: false);
    }
}

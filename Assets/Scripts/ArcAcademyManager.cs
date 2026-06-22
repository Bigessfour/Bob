using UnityEngine;

/// <summary>
/// Arc Academy environment controller — episode randomization, spawn, and score feedback.
/// </summary>
public class ArcAcademyManager : MonoBehaviour
{
    [SerializeField] private MovableHoop movableHoop;
    [SerializeField] private Transform spawnPad;
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private ArcAcademyScorePopup scorePopup;
    [SerializeField] private SpawnPadPulse spawnPadPulse;
    [SerializeField] private int curriculumStage = 1;

    [Tooltip("When false, hoop and spawn stay at regulation defaults every episode (Week 1 training).")]
    [SerializeField] private bool randomizeEpisodeLayout;

    private static ArcAcademyManager instance;
    private int sessionMadeBaskets;
    private bool firstEpisode = true;

    public static ArcAcademyManager Instance => instance;

    public int CurriculumStage => curriculumStage;

    public int SessionMadeBaskets => sessionMadeBaskets;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("ArcAcademyManager: duplicate instance ignored.");
            return;
        }

        instance = this;

        if (movableHoop == null)
        {
            movableHoop = GetComponentInChildren<MovableHoop>();
        }

        if (spawnPad == null)
        {
            var pad = GameObject.Find(ArcAcademyLayout.SpawnPadName);
            if (pad != null)
            {
                spawnPad = pad.transform;
            }
        }

        if (ballSpawnPoint == null && spawnPad != null)
        {
            ballSpawnPoint = spawnPad.Find(ArcAcademyLayout.BallSpawnPointName);
        }

        if (scorePopup == null)
        {
            scorePopup = GetComponentInChildren<ArcAcademyScorePopup>();
        }

        if (spawnPadPulse == null && spawnPad != null)
        {
            spawnPadPulse = spawnPad.GetComponent<SpawnPadPulse>();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void WireReferences(MovableHoop hoop, Transform pad, Transform spawnPoint, ArcAcademyScorePopup popup)
    {
        movableHoop = hoop;
        spawnPad = pad;
        ballSpawnPoint = spawnPoint;
        scorePopup = popup;

        if (spawnPad != null && spawnPadPulse == null)
        {
            spawnPadPulse = spawnPad.GetComponent<SpawnPadPulse>();
        }
    }

    public bool RandomizeEpisodeLayout => randomizeEpisodeLayout;

    public void SetupForTraining()
    {
        if (movableHoop != null)
        {
            movableHoop.ApplyDefaultPose();
        }
    }

    /// <summary>
    /// Resets the arena for a new episode. Stable layout by default; randomize only when curriculum is enabled.
    /// </summary>
    public void PrepareEpisode(int stage = -1)
    {
        if (randomizeEpisodeLayout)
        {
            RandomizeEpisode(stage);
        }
        else
        {
            SetupForTraining();
        }
    }

    public void RandomizeEpisode(int stage = -1)
    {
        int activeStage = stage >= 0 ? stage : curriculumStage;
        movableHoop?.RandomizePose(activeStage);
    }

    public Vector3 GetSpawnPosition()
    {
        if (ballSpawnPoint != null)
        {
            return ResolveSpawnPosition(ballSpawnPoint.position);
        }

        Vector3 basePos = spawnPad != null
            ? spawnPad.position + ArcAcademyLayout.BobSpawnOffset
            : ArcAcademyLayout.BobSpawnPosition;

        return ResolveSpawnPosition(basePos);
    }

    private Vector3 ResolveSpawnPosition(Vector3 basePos)
    {
        if (!randomizeEpisodeLayout)
        {
            return basePos;
        }

        float xJitter = Random.Range(
            -ArcAcademyLayout.SpawnLateralJitter,
            ArcAcademyLayout.SpawnLateralJitter);

        return basePos + new Vector3(xJitter, 0f, 0f);
    }

    public void NotifyEpisodeBegin(BobAgent agent, System.Action onReady)
    {
        PrepareEpisode();
        Vector3 spawn = GetSpawnPosition();
        var entrance = agent.GetComponent<BobEntranceController>();

        if (entrance == null)
        {
            agent.ApplySpawn(spawn);
            TriggerSpawnReady();
            onReady?.Invoke();
            return;
        }

        if (firstEpisode)
        {
            firstEpisode = false;
            entrance.PlaySessionIntro(spawn, () =>
            {
                agent.ApplySpawn(spawn);
                onReady?.Invoke();
            });
        }
        else
        {
            entrance.PlayEpisodeReset(spawn, () =>
            {
                agent.ApplySpawn(spawn);
                onReady?.Invoke();
            });
        }
    }

    public void TriggerSpawnReady()
    {
        spawnPadPulse?.TriggerReadyPulse();
    }

    public void SetCurriculumStage(int stage)
    {
        curriculumStage = Mathf.Max(1, stage);
    }

    public void NotifyMadeBasket(BobAgent agent, bool swish)
    {
        sessionMadeBaskets++;
        BobTrainingStats.Instance?.RecordBasketballPoint();
        scorePopup?.Show(swish, sessionMadeBaskets);
        spawnPadPulse?.TriggerScoreBurst();

        Debug.Log(swish
            ? $"Arc Academy swish #{sessionMadeBaskets} — Bob nets a perfect shot!"
            : $"Arc Academy score #{sessionMadeBaskets} — Bob sunk it!");

        agent.RegisterMadeShot(swish);
    }

    public void NotifyBackboardHit(float impactSpeed)
    {
        if (impactSpeed > 1.5f)
        {
            Debug.Log($"Backboard clank — impact {impactSpeed:F1} m/s");
        }
    }
}

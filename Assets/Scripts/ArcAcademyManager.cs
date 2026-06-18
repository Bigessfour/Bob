using UnityEngine;

/// <summary>
/// Arc Academy environment controller — episode randomization for movable hoop and spawn pad.
/// </summary>
public class ArcAcademyManager : MonoBehaviour
{
    [SerializeField] private MovableHoop movableHoop;
    [SerializeField] private Transform spawnPad;
    [SerializeField] private int curriculumStage = 1;

    [Tooltip("When false, hoop and spawn stay at regulation defaults every episode (Week 1 training).")]
    [SerializeField] private bool randomizeEpisodeLayout;

    private static ArcAcademyManager instance;

    public static ArcAcademyManager Instance => instance;

    public int CurriculumStage => curriculumStage;

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
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void WireReferences(MovableHoop hoop, Transform pad)
    {
        movableHoop = hoop;
        spawnPad = pad;
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
        Vector3 basePos = spawnPad != null
            ? spawnPad.position + ArcAcademyLayout.BobSpawnOffset
            : ArcAcademyLayout.BobSpawnPosition;

        if (!randomizeEpisodeLayout)
        {
            return basePos;
        }

        float xJitter = Random.Range(
            -ArcAcademyLayout.SpawnLateralJitter,
            ArcAcademyLayout.SpawnLateralJitter);

        return basePos + new Vector3(xJitter, 0f, 0f);
    }

    public void SetCurriculumStage(int stage)
    {
        curriculumStage = Mathf.Max(1, stage);
    }
}

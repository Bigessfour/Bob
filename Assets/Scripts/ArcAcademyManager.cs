using UnityEngine;

/// <summary>
/// Arc Academy environment controller — episode randomization for movable hoop and spawn pad.
/// </summary>
public class ArcAcademyManager : MonoBehaviour
{
    [SerializeField] private MovableHoop movableHoop;
    [SerializeField] private Transform spawnPad;
    [SerializeField] private int curriculumStage = 1;

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

    public void SetupForTraining()
    {
        if (movableHoop != null)
        {
            movableHoop.ApplyDefaultPose();
        }
    }

    public void RandomizeEpisode(int stage = -1)
    {
        int activeStage = stage >= 0 ? stage : curriculumStage;
        movableHoop?.RandomizePose(activeStage);
    }

    public Vector3 GetRandomSpawnPosition()
    {
        float xJitter = Random.Range(
            -ArcAcademyLayout.SpawnLateralJitter,
            ArcAcademyLayout.SpawnLateralJitter);

        Vector3 basePos = spawnPad != null
            ? spawnPad.position + ArcAcademyLayout.BobSpawnOffset
            : ArcAcademyLayout.BobSpawnPosition;

        return basePos + new Vector3(xJitter, 0f, 0f);
    }

    public void SetCurriculumStage(int stage)
    {
        curriculumStage = Mathf.Max(1, stage);
    }
}

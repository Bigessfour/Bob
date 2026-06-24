using UnityEngine;

/// <summary>
/// AI Warehouse–style arena orchestrator for a single Bob agent.
/// Does not mass-instantiate agents at runtime — wiring and spawn live here;
/// reward/observation logic stays on <see cref="BobAgent"/>.
/// </summary>
public class SimpleArcArenaManager : MonoBehaviour
{
    [Header("AI Warehouse Style Settings")]
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private BobAgent agent;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Vector3 bobSpawnOffset = ArcAcademyLayout.BobSpawnOffset;

    private static SimpleArcArenaManager instance;

    public static SimpleArcArenaManager Instance => instance;

    public GameObject AgentPrefab => agentPrefab;

    public Transform PrimarySpawnPoint =>
        spawnPoints != null && spawnPoints.Length > 0 ? spawnPoints[0] : null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("SimpleArcArenaManager: duplicate instance ignored.");
            return;
        }

        instance = this;
        ResolveAgent();
        EnsureAgentParented();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public Vector3 GetBobSpawnPosition()
    {
        if (PrimarySpawnPoint != null)
        {
            return ResolveSpawnPosition(
                SimpleArcAcademyArena.GetLabBobSpawnPosition(PrimarySpawnPoint));
        }

        return ResolveSpawnPosition(ArcAcademyLayout.BobSpawnPosition);
    }

    public Quaternion GetBobSpawnRotation()
    {
        Transform hoop = agent != null ? agent.hoop : null;
        return SimpleArcAcademyArena.GetSpawnFacingRotation(GetBobSpawnPosition(), hoop);
    }

    /// <summary>Manual/orchestrator episode reset — ML-Agents also resets via OnEpisodeBegin.</summary>
    public void ResetEpisode()
    {
        ArcAcademyManager.Instance?.PrepareEpisode();
        if (agent != null)
        {
            agent.ApplySpawn(GetBobSpawnPosition(), GetBobSpawnRotation());
        }
    }

    public void Wire(BobAgent bobAgent, Transform spawnPoint, GameObject prefab = null)
    {
        agent = bobAgent;
        spawnPoints = spawnPoint != null ? new[] { spawnPoint } : null;
        if (prefab != null)
        {
            agentPrefab = prefab;
        }
    }

    /// <summary>Lab mode: Bob stands on the hardwood at the free-throw line (no pedestal).</summary>
    public void ConfigureLabFloorSpawn(Vector3 floorOffset)
    {
        bobSpawnOffset = floorOffset;
    }

    private void ResolveAgent()
    {
        if (agent == null)
        {
            agent = FindAnyObjectByType<BobAgent>();
        }
    }

    private void EnsureAgentParented()
    {
        if (agent == null)
        {
            return;
        }

        if (agent.transform.parent != transform)
        {
            agent.transform.SetParent(transform, true);
        }
    }

    private static Vector3 ResolveSpawnPosition(Vector3 basePos)
    {
        var manager = ArcAcademyManager.Instance;
        if (manager == null || !manager.RandomizeEpisodeLayout)
        {
            return basePos;
        }

        float xJitter = Random.Range(
            -ArcAcademyLayout.SpawnLateralJitter,
            ArcAcademyLayout.SpawnLateralJitter);

        return basePos + new Vector3(xJitter, 0f, 0f);
    }
}

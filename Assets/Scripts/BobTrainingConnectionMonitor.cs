using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

/// <summary>
/// Detects ML-Agents inference fallback (no Python trainer on port 5004) and surfaces a clear warning.
/// Applies training time scale when the communicator connects (matches config engine_settings.time_scale).
/// </summary>
public class BobTrainingConnectionMonitor : MonoBehaviour
{
    [SerializeField] private float trainingTimeScale = 20f;
    [SerializeField] private float checkIntervalSeconds = 0.5f;

    public bool IsTrainingConnected { get; private set; }

    public static BobTrainingConnectionMonitor Instance { get; private set; }

    public string StatusLabel =>
        IsTrainingConnected ? "Training (PPO)" : "Inference fallback — start ./scripts/train.sh";

    private BehaviorParameters bobBehavior;
    private float nextCheckTime;
    private bool loggedFallback;
    private bool hadTrainingConnection;
    private float defaultTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        defaultTimeScale = Time.timeScale;
    }

    private void Start()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob != null)
        {
            bobBehavior = bob.GetComponent<BehaviorParameters>();
        }
    }

    private void Update()
    {
        if (Time.time < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.time + checkIntervalSeconds;
        RefreshConnectionState();
    }

    private void OnDestroy()
    {
        Time.timeScale = defaultTimeScale;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void RefreshConnectionState()
    {
        if (bobBehavior == null || bobBehavior.BehaviorType != BehaviorType.Default)
        {
            IsTrainingConnected = false;
            return;
        }

        bool connected = Academy.Instance != null && Academy.Instance.IsCommunicatorOn;
        if (connected == IsTrainingConnected)
        {
            return;
        }

        IsTrainingConnected = connected;
        if (connected)
        {
            Time.timeScale = trainingTimeScale;
            hadTrainingConnection = true;
            BobTrainingSessionFlags.MarkTrainerConnected();
            Debug.Log($"BOB_TRAINING_OK: Python trainer connected. Time scale = {trainingTimeScale}x");
            loggedFallback = false;
            return;
        }

        Time.timeScale = defaultTimeScale;
        if (hadTrainingConnection)
        {
            Debug.LogWarning(
                "BOB_TRAINING_LOST: Python trainer disconnected (Play stopped, domain reload, or compile). " +
                "Trainer terminal may show 'Communicator has exited'. Wait for 'Listening on port 5004', then press Play once — " +
                "do not toggle Play repeatedly or edit scripts during training.");
            hadTrainingConnection = false;
            return;
        }

        if (!loggedFallback)
        {
            Debug.LogWarning(
                "BOB_TRAINING_WARN: No Python trainer on port 5004. Bob is in inference/heuristic fallback. " +
                "Run ./scripts/train.sh, wait for 'Listening on port 5004', stop Play, then press Play again.");
            loggedFallback = true;
        }
    }
}

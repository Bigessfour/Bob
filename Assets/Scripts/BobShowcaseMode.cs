using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using UnityEngine;

/// <summary>
/// Honest Play-mode label: InferenceOnly + ONNX vs Heuristic solver vs live PPO.
/// The old HUD string "Inference fallback" was a lie — Default without a trainer
/// is not a trained policy. Classmate demos must show this chip.
/// </summary>
public class BobShowcaseMode : MonoBehaviour
{
    public enum Kind
    {
        Unknown,
        TrainingPpo,
        InferenceOnnx,
        HeuristicSolver,
        RecordingDemos,
        DefaultNoTrainer,
    }

    public static BobShowcaseMode Instance { get; private set; }

    public Kind Current { get; private set; } = Kind.Unknown;

    public string HudLabel { get; private set; } = "Mode ?";

    public string ModelName { get; private set; } = "—";

    public bool IsHonestInference => Current == Kind.InferenceOnnx;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshAndLog();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RefreshAndLog()
    {
        Refresh();
        switch (Current)
        {
            case Kind.InferenceOnnx:
                Debug.Log($"BOB_INFERENCE_OK: BehaviorType=InferenceOnly model={ModelName}");
                break;
            case Kind.HeuristicSolver:
                Debug.LogWarning(
                    "BOB_SHOWCASE_WARN: HeuristicOnly — shots are the analytic solver (c≈0), not the ONNX policy. " +
                    "Use Bob → Demo → Prepare Classmate Showcase before claiming learned makes.");
                break;
            case Kind.RecordingDemos:
                Debug.LogWarning("BOB_SHOWCASE_WARN: DemonstrationRecorder is on — this is BC capture, not inference.");
                break;
            case Kind.DefaultNoTrainer:
                Debug.LogWarning(
                    "BOB_SHOWCASE_WARN: BehaviorType=Default and no Python trainer. " +
                    "This is NOT a trained-policy demo. Bob → Demo → Prepare Classmate Showcase.");
                break;
            case Kind.TrainingPpo:
                break;
            default:
                Debug.LogWarning("BOB_SHOWCASE_WARN: Could not resolve Bob BehaviorParameters.");
                break;
        }
    }

    public void Refresh()
    {
        var snapshot = Evaluate();
        Current = snapshot.kind;
        HudLabel = snapshot.label;
        ModelName = snapshot.modelName;
    }

    public static Snapshot Evaluate()
    {
        var agent = Object.FindAnyObjectByType<BobAgent>();
        if (agent == null)
        {
            return new Snapshot(Kind.Unknown, "Mode ?", "—");
        }

        var recorder = agent.GetComponent<DemonstrationRecorder>();
        if (recorder != null && recorder.Record)
        {
            return new Snapshot(Kind.RecordingDemos, "REC demos (not ML)", "—");
        }

        var behavior = agent.GetComponent<BehaviorParameters>();
        if (behavior == null)
        {
            return new Snapshot(Kind.Unknown, "Mode ?", "—");
        }

        string modelName = behavior.Model != null ? behavior.Model.name : "—";

        if (behavior.BehaviorType == BehaviorType.InferenceOnly)
        {
            if (behavior.Model == null)
            {
                return new Snapshot(Kind.Unknown, "INFERENCE FAIL (no ONNX)", "—");
            }

            return new Snapshot(Kind.InferenceOnnx, $"INFERENCE · {modelName}", modelName);
        }

        if (behavior.BehaviorType == BehaviorType.HeuristicOnly)
        {
            return new Snapshot(Kind.HeuristicSolver, "SOLVER (heuristic c≈0)", modelName);
        }

        var monitor = BobTrainingConnectionMonitor.Instance;
        if (monitor != null && monitor.IsTrainingConnected)
        {
            return new Snapshot(Kind.TrainingPpo, "Training (PPO)", modelName);
        }

        return new Snapshot(Kind.DefaultNoTrainer, "Default · no trainer", modelName);
    }

    public static Color HudColor(Kind kind)
    {
        return kind switch
        {
            Kind.InferenceOnnx => new Color(0.35f, 0.92f, 0.55f, 1f),
            Kind.TrainingPpo => new Color(0.45f, 0.78f, 1f, 1f),
            Kind.HeuristicSolver => new Color(1f, 0.72f, 0.22f, 1f),
            Kind.RecordingDemos => new Color(1f, 0.45f, 0.85f, 1f),
            _ => new Color(0.95f, 0.32f, 0.32f, 1f),
        };
    }

    public readonly struct Snapshot
    {
        public readonly Kind kind;
        public readonly string label;
        public readonly string modelName;

        public Snapshot(Kind kind, string label, string modelName)
        {
            this.kind = kind;
            this.label = label;
            this.modelName = modelName;
        }
    }
}

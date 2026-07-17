using UnityEngine;

/// <summary>
/// Lightweight game-state tracker for training, demo, and pause flows (Cuphead-style polish path).
/// </summary>
public class BobGameStateMachine : MonoBehaviour
{
    public enum BobGameState
    {
        Training,
        Demo,
        Paused
    }

    public static BobGameStateMachine Instance { get; private set; }

    [SerializeField] private BobGameState currentState = BobGameState.Training;

    public BobGameState CurrentState => currentState;

    public bool IsTraining => currentState == BobGameState.Training;
    public bool IsDemo => currentState == BobGameState.Demo;
    public bool IsPaused => currentState == BobGameState.Paused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetTraining()
    {
        currentState = BobGameState.Training;
    }

    public void SetDemo()
    {
        currentState = BobGameState.Demo;
    }

    public void SetPaused(bool paused)
    {
        currentState = paused ? BobGameState.Paused : BobGameState.Training;
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }
}

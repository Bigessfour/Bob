using UnityEngine;

/// <summary>
/// Diegetic SFX for bounce / swish / score / miss — commercial juice for the lab demo.
/// Clips live under Assets/Audio/; missing clips fail soft (no throw).
/// </summary>
public class BobAudioFeedback : MonoBehaviour
{
    public const string RootName = "BobAudioFeedback";

    public static BobAudioFeedback Instance { get; private set; }

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip bounceClip;
    [SerializeField] private AudioClip swishClip;
    [SerializeField] private AudioClip scoreClip;
    [SerializeField] private AudioClip missClip;

    [SerializeField] private float bounceCooldown = 0.08f;

    private float lastBounceTime = -10f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        if (source == null)
        {
            source = gameObject.GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlayBounce()
    {
        if (Time.unscaledTime - lastBounceTime < bounceCooldown)
        {
            return;
        }

        lastBounceTime = Time.unscaledTime;
        Play(bounceClip, 0.55f);
    }

    public void PlaySwish()
    {
        Play(swishClip, 0.85f);
    }

    public void PlayScore()
    {
        Play(scoreClip, 0.9f);
    }

    public void PlayMiss()
    {
        Play(missClip, 0.65f);
    }

    private void Play(AudioClip clip, float volume)
    {
        if (source == null || clip == null)
        {
            return;
        }

        source.PlayOneShot(clip, volume);
    }

    public void AssignClips(AudioClip bounce, AudioClip swish, AudioClip score, AudioClip miss)
    {
        bounceClip = bounce;
        swishClip = swish;
        scoreClip = score;
        missClip = miss;
    }
}

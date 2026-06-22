using UnityEngine;

/// <summary>
/// Procedural squash/stretch on Bob when launching the basketball.
/// </summary>
public class BobProceduralAnimator : MonoBehaviour
{
    [SerializeField] private Vector3 squashScale = new(1.08f, 0.85f, 1.08f);
    [SerializeField] private Vector3 stretchScale = new(0.95f, 1.12f, 0.95f);
    [SerializeField] private float transitionSpeed = 8f;

    private BobAgent agent;
    private Vector3 baseScale = Vector3.one;
    private bool shotImpulseActive;
    private float squashTimer;

    private void Awake()
    {
        agent = GetComponent<BobAgent>();
        baseScale = transform.localScale;
    }

    public void NotifyShotImpulse()
    {
        shotImpulseActive = true;
        squashTimer = 0.12f;
        transform.localScale = Vector3.Scale(baseScale, squashScale);
    }

    private void LateUpdate()
    {
        if (squashTimer > 0f)
        {
            squashTimer -= Time.deltaTime;
            if (squashTimer <= 0f)
            {
                transform.localScale = Vector3.Scale(baseScale, squashScale);
            }

            return;
        }

        if (!shotImpulseActive || agent == null)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * transitionSpeed);
            return;
        }

        var body = agent.ProjectileBody;
        if (body != null && body.linearVelocity.y > 0.2f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.Scale(baseScale, stretchScale),
                Time.deltaTime * transitionSpeed);
            return;
        }

        if (body == null || body.linearVelocity.sqrMagnitude < 0.08f)
        {
            shotImpulseActive = false;
        }

        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * transitionSpeed);
    }
}

using UnityEngine;

/// <summary>
/// Simple eye expressions on Bob's Eye_Left / Eye_Right quads.
/// </summary>
public class BobFaceExpression : MonoBehaviour
{
    private static readonly Vector3 DefaultEyeScale = new(0.14f, 0.2f, 1f);
    private static readonly Vector3 FocusEyeScale = new(0.08f, 0.2f, 1f);
    private static readonly Vector3 HappyEyeScale = new(0.16f, 0.22f, 1f);
    private static readonly Vector3 SurprisedEyeScale = new(0.2f, 0.26f, 1f);

    [SerializeField] private float expressionHoldSeconds = 0.45f;

    private Transform eyeLeft;
    private Transform eyeRight;
    private Vector3 targetScale = DefaultEyeScale;
    private float holdTimer;

    private void Awake()
    {
        eyeLeft = transform.Find("Eye_Left");
        eyeRight = transform.Find("Eye_Right");
        ApplyScale(DefaultEyeScale);
    }

    public void SetFocus()
    {
        SetExpression(FocusEyeScale);
    }

    public void SetHappy()
    {
        SetExpression(HappyEyeScale);
    }

    public void SetSurprised()
    {
        SetExpression(SurprisedEyeScale);
    }

    public void OnEpisodeEnded(bool scored)
    {
        if (!scored)
        {
            SetSurprised();
        }
    }

    private void SetExpression(Vector3 scale)
    {
        targetScale = scale;
        holdTimer = expressionHoldSeconds;
        ApplyScale(scale);
    }

    private void Update()
    {
        if (holdTimer <= 0f)
        {
            targetScale = DefaultEyeScale;
        }
        else
        {
            holdTimer -= Time.deltaTime;
        }

        LerpEyesTo(targetScale);
    }

    private void LerpEyesTo(Vector3 scale)
    {
        if (eyeLeft != null)
        {
            eyeLeft.localScale = Vector3.Lerp(eyeLeft.localScale, scale, Time.deltaTime * 10f);
        }

        if (eyeRight != null)
        {
            eyeRight.localScale = Vector3.Lerp(eyeRight.localScale, scale, Time.deltaTime * 10f);
        }
    }

    private void ApplyScale(Vector3 scale)
    {
        if (eyeLeft != null)
        {
            eyeLeft.localScale = scale;
        }

        if (eyeRight != null)
        {
            eyeRight.localScale = scale;
        }
    }
}

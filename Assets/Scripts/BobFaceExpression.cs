using UnityEngine;

/// <summary>
/// Simple eye expressions on Bob's LeftEye / RightEye sphere pivots.
/// </summary>
public class BobFaceExpression : MonoBehaviour
{
    [SerializeField] private float expressionHoldSeconds = 0.45f;

    private Transform eyeLeft;
    private Transform eyeRight;
    private Transform scleraLeft;
    private Transform scleraRight;
    private float targetScale = BobFaceLayout.DefaultExpressionScale;
    private float holdTimer;

    private void Awake()
    {
        eyeLeft = FindEye(BobFaceLayout.LeftEyeName, "Eye_Left");
        eyeRight = FindEye(BobFaceLayout.RightEyeName, "Eye_Right");
        scleraLeft = eyeLeft != null ? eyeLeft.Find(BobFaceLayout.ScleraName) : null;
        scleraRight = eyeRight != null ? eyeRight.Find(BobFaceLayout.ScleraName) : null;
        ApplyScale(BobFaceLayout.DefaultExpressionScale);
    }

    public void SetFocus()
    {
        SetExpression(BobFaceLayout.FocusExpressionScale);
    }

    public void SetHappy()
    {
        SetExpression(BobFaceLayout.HappyExpressionScale);
    }

    public void SetSurprised()
    {
        SetExpression(BobFaceLayout.SurprisedExpressionScale);
    }

    public void OnEpisodeEnded(bool scored)
    {
        if (!scored)
        {
            SetSurprised();
        }
    }

    private Transform FindEye(string primaryName, string legacyName)
    {
        var eye = transform.Find(primaryName);
        return eye != null ? eye : transform.Find(legacyName);
    }

    private void SetExpression(float scale)
    {
        targetScale = scale;
        holdTimer = expressionHoldSeconds;
        ApplyScale(scale);
    }

    private void Update()
    {
        if (holdTimer <= 0f)
        {
            targetScale = BobFaceLayout.DefaultExpressionScale;
        }
        else
        {
            holdTimer -= Time.deltaTime;
        }

        LerpEyesTo(targetScale);
    }

    private void LerpEyesTo(float scale)
    {
        LerpEyeScale(eyeLeft, scleraLeft, scale);
        LerpEyeScale(eyeRight, scleraRight, scale);
    }

    private void ApplyScale(float scale)
    {
        SetEyeScale(eyeLeft, scleraLeft, scale);
        SetEyeScale(eyeRight, scleraRight, scale);
    }

    private static void LerpEyeScale(Transform eyeRoot, Transform sclera, float scale)
    {
        if (eyeRoot == null)
        {
            return;
        }

        var target = ScaleForExpression(sclera, scale);
        var current = sclera != null ? sclera.localScale : eyeRoot.localScale;
        var next = Vector3.Lerp(current, target, Time.deltaTime * 10f);
        if (sclera != null)
        {
            sclera.localScale = next;
        }
        else
        {
            eyeRoot.localScale = next;
        }
    }

    private static void SetEyeScale(Transform eyeRoot, Transform sclera, float scale)
    {
        if (eyeRoot == null)
        {
            return;
        }

        var applied = ScaleForExpression(sclera, scale);
        if (sclera != null)
        {
            sclera.localScale = applied;
        }
        else
        {
            eyeRoot.localScale = applied;
        }
    }

    private static Vector3 ScaleForExpression(Transform sclera, float scale)
    {
        if (sclera != null)
        {
            return BobFaceLayout.ScleraLocalScale * scale;
        }

        return Vector3.one * scale;
    }
}

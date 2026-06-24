using UnityEngine;

/// <summary>
/// Rotates Bob's LeftEye and RightEye toward the basketball each frame.
/// </summary>
public class BobEyeFollow : MonoBehaviour
{
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private Transform ballTarget;
    [SerializeField] private LineRenderer mouth;

    private void Awake()
    {
        if (leftEye == null)
        {
            leftEye = transform.Find(BobFaceLayout.LeftEyeName);
        }

        if (rightEye == null)
        {
            rightEye = transform.Find(BobFaceLayout.RightEyeName);
        }

        if (mouth == null)
        {
            var mouthTransform = transform.Find(BobFaceLayout.MouthName);
            if (mouthTransform != null)
            {
                mouth = mouthTransform.GetComponent<LineRenderer>();
            }
        }

        if (ballTarget == null)
        {
            var agent = GetComponent<BobAgent>();
            if (agent != null && agent.ProjectileBody != null)
            {
                ballTarget = agent.ProjectileBody.transform;
            }
            else
            {
                var ball = GameObject.Find(BasketballProjectileSetup.BasketballName);
                if (ball != null)
                {
                    ballTarget = ball.transform;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (ballTarget == null)
        {
            return;
        }

        if (leftEye != null)
        {
            leftEye.LookAt(ballTarget);
        }

        if (rightEye != null)
        {
            rightEye.LookAt(ballTarget);
        }
    }
}

using UnityEngine;

/// <summary>
/// Robotic hoop assembly with swivel base and arm articulation.
/// Default pose is fixed regulation height; randomize via ArcAcademyManager when curriculum is enabled.
/// </summary>
public class MovableHoop : MonoBehaviour
{
    [SerializeField] private Transform rimTransform;
    [SerializeField] private Transform swivelLink;
    [SerializeField] private Transform armLink;
    [SerializeField] private ArticulationBody swivelBody;
    [SerializeField] private ArticulationBody armBody;
    [SerializeField] private Vector3 defaultRootPosition = ArcAcademyLayout.HoopRootDefaultPosition;
    [SerializeField] private Vector3 defaultRimLocalPosition = ArcAcademyLayout.RimLocalDefaultPosition;
    [SerializeField] private float poseLerpSpeed = 4f;

    private float targetSwivelYaw;
    private float targetArmPitch;
    private float currentSwivelYaw;
    private float currentArmPitch;
    private float targetRimLocalY;
    private float currentRimLocalY;

    public Transform RimTransform => rimTransform;

    private void Awake()
    {
        if (rimTransform == null)
        {
            var rim = transform.Find($"{ArcAcademyLayout.RimName}");
            if (rim == null)
            {
                rim = transform.Find($"HoopHead/{ArcAcademyLayout.RimName}");
            }

            if (rim != null)
            {
                rimTransform = rim;
            }
        }

        if (swivelLink == null)
        {
            swivelLink = transform.Find("RoboticSwivelBase/SwivelLink");
        }

        if (armLink == null)
        {
            armLink = transform.Find("RoboticSwivelBase/SwivelLink/ArmLink");
        }

        if (swivelBody == null && swivelLink != null)
        {
            swivelBody = swivelLink.GetComponent<ArticulationBody>();
        }

        if (armBody == null && armLink != null)
        {
            armBody = armLink.GetComponent<ArticulationBody>();
        }

        targetRimLocalY = defaultRimLocalPosition.y;
        currentRimLocalY = targetRimLocalY;
    }

    public void SetRimTransform(Transform rim)
    {
        rimTransform = rim;
    }

    public void WireArticulation(Transform swivel, Transform arm, ArticulationBody swivelArticulation, ArticulationBody armArticulation)
    {
        swivelLink = swivel;
        armLink = arm;
        swivelBody = swivelArticulation;
        armBody = armArticulation;
    }

    public void ApplyDefaultPose()
    {
        transform.position = defaultRootPosition;
        targetSwivelYaw = 0f;
        targetArmPitch = 0f;
        targetRimLocalY = defaultRimLocalPosition.y;
        SnapPoseIfNeeded();
    }

    public void RandomizePose(int curriculumStage)
    {
        float stageScale = ArcAcademyLayout.GetStageHoopOffsetScale(curriculumStage);
        float offsetX = Random.Range(-ArcAcademyLayout.MaxHoopOffsetX, ArcAcademyLayout.MaxHoopOffsetX) * stageScale;
        float offsetZ = Random.Range(-ArcAcademyLayout.MaxHoopOffsetZ, ArcAcademyLayout.MaxHoopOffsetZ) * stageScale;
        transform.position = defaultRootPosition + new Vector3(offsetX, 0f, offsetZ);

        targetSwivelYaw = Random.Range(-18f, 18f) * stageScale;
        targetArmPitch = Random.Range(-8f, 8f) * stageScale;
        targetRimLocalY = Mathf.Lerp(
            ArcAcademyLayout.MinRimHeight,
            ArcAcademyLayout.MaxRimHeight,
            Random.Range(0f, stageScale));
    }

    private void Update()
    {
        float step = Time.deltaTime * poseLerpSpeed;
        currentSwivelYaw = Mathf.Lerp(currentSwivelYaw, targetSwivelYaw, step);
        currentArmPitch = Mathf.Lerp(currentArmPitch, targetArmPitch, step);
        currentRimLocalY = Mathf.Lerp(currentRimLocalY, targetRimLocalY, step);

        ApplySwivel(currentSwivelYaw);
        ApplyArmPitch(currentArmPitch);

        if (rimTransform != null)
        {
            var local = rimTransform.localPosition;
            local.y = currentRimLocalY;
            rimTransform.localPosition = local;
        }
    }

    private void SnapPoseIfNeeded()
    {
        if (!Application.isPlaying)
        {
            currentSwivelYaw = targetSwivelYaw;
            currentArmPitch = targetArmPitch;
            currentRimLocalY = targetRimLocalY;
            ApplySwivel(currentSwivelYaw);
            ApplyArmPitch(currentArmPitch);

            if (rimTransform != null)
            {
                var local = rimTransform.localPosition;
                local.y = currentRimLocalY;
                rimTransform.localPosition = local;
            }
        }
    }

    private void ApplySwivel(float yawDegrees)
    {
        if (swivelBody != null)
        {
            var drive = swivelBody.xDrive;
            drive.stiffness = 80000f;
            drive.damping = 2000f;
            drive.target = yawDegrees;
            swivelBody.xDrive = drive;
        }
        else if (swivelLink != null)
        {
            swivelLink.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
        }
    }

    private void ApplyArmPitch(float pitchDegrees)
    {
        if (armBody != null)
        {
            var drive = armBody.xDrive;
            drive.stiffness = 80000f;
            drive.damping = 2000f;
            drive.target = pitchDegrees;
            armBody.xDrive = drive;
        }
        else if (armLink != null)
        {
            armLink.localRotation = Quaternion.Euler(pitchDegrees, 0f, 0f);
        }
    }
}

using UnityEngine;

/// <summary>
/// Robotic hoop assembly with swivel base and arm articulation.
/// Default pose is fixed regulation height; randomize via ArcAcademyManager when curriculum is enabled.
/// Week 1 training locks the hoop stationary via <see cref="SetStationaryForTraining"/>.
/// </summary>
public class MovableHoop : MonoBehaviour
{
    [SerializeField] private Transform rimTransform;
    [SerializeField] private Transform hoopHeadTransform;
    [SerializeField] private Transform swivelLink;
    [SerializeField] private Transform armLink;
    [SerializeField] private ArticulationBody swivelBody;
    [SerializeField] private ArticulationBody armBody;
    [SerializeField] private Vector3 defaultRootPosition = ArcAcademyLayout.HoopRootDefaultPosition;
    [SerializeField] private Vector3 defaultRimLocalPosition = ArcAcademyLayout.RimLocalOnHoopHead;
    [SerializeField] private float poseLerpSpeed = 4f;
    [SerializeField] private bool stationaryForTraining = true;

    private float targetSwivelYaw;
    private float targetArmPitch;
    private float currentSwivelYaw;
    private float currentArmPitch;

    public Transform RimTransform => rimTransform;

    public bool IsStationaryForTraining => stationaryForTraining;

    public void SetStationaryForTraining(bool stationary)
    {
        stationaryForTraining = stationary;
        if (stationary)
        {
            ApplyDefaultPose();
            SnapPoseImmediate();
        }
    }

    private void Awake()
    {
        ResolveReferences();
        SnapPoseImmediate();
    }

    private void ResolveReferences()
    {
        if (hoopHeadTransform == null)
        {
            hoopHeadTransform = transform.Find("HoopHead");
            if (hoopHeadTransform == null)
            {
                hoopHeadTransform = TrainingHoopDetail.FindRim(transform)?.parent;
            }
        }

        if (rimTransform == null)
        {
            rimTransform = TrainingHoopDetail.FindRim(transform);
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

        if (hoopHeadTransform != null && hoopHeadTransform.parent == transform)
        {
            hoopHeadTransform.localPosition = ArcAcademyLayout.StationaryHoopHeadLocalPosition;
            hoopHeadTransform.localRotation = Quaternion.identity;
        }

        if (rimTransform != null)
        {
            rimTransform.localPosition = defaultRimLocalPosition;
        }
    }

    public void RandomizePose(int curriculumStage)
    {
        if (stationaryForTraining)
        {
            ApplyDefaultPose();
            return;
        }

        float stageScale = ArcAcademyLayout.GetStageHoopOffsetScale(curriculumStage);
        float offsetX = Random.Range(-ArcAcademyLayout.MaxHoopOffsetX, ArcAcademyLayout.MaxHoopOffsetX) * stageScale;
        float offsetZ = Random.Range(-ArcAcademyLayout.MaxHoopOffsetZ, ArcAcademyLayout.MaxHoopOffsetZ) * stageScale;
        transform.position = defaultRootPosition + new Vector3(offsetX, 0f, offsetZ);

        targetSwivelYaw = Random.Range(-18f, 18f) * stageScale;
        targetArmPitch = Random.Range(-8f, 8f) * stageScale;
    }

    private void Update()
    {
        if (stationaryForTraining)
        {
            return;
        }

        float step = Time.deltaTime * poseLerpSpeed;
        currentSwivelYaw = Mathf.Lerp(currentSwivelYaw, targetSwivelYaw, step);
        currentArmPitch = Mathf.Lerp(currentArmPitch, targetArmPitch, step);

        ApplySwivel(currentSwivelYaw);
        ApplyArmPitch(currentArmPitch);
    }

    private void SnapPoseImmediate()
    {
        currentSwivelYaw = targetSwivelYaw;
        currentArmPitch = targetArmPitch;
        ApplySwivel(currentSwivelYaw);
        ApplyArmPitch(currentArmPitch);

        if (rimTransform != null)
        {
            rimTransform.localPosition = defaultRimLocalPosition;
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

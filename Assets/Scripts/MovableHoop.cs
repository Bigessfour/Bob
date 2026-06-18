using UnityEngine;

/// <summary>
/// Robotic hoop assembly — randomizes rim pose each episode within Arc Academy ranges.
/// </summary>
public class MovableHoop : MonoBehaviour
{
    [SerializeField] private Transform rimTransform;
    [SerializeField] private Vector3 defaultRootPosition = ArcAcademyLayout.HoopRootDefaultPosition;
    [SerializeField] private Vector3 defaultRimLocalPosition = ArcAcademyLayout.RimLocalDefaultPosition;

    public Transform RimTransform => rimTransform;

    private void Awake()
    {
        if (rimTransform == null)
        {
            var rim = transform.Find(ArcAcademyLayout.RimName);
            if (rim != null)
            {
                rimTransform = rim;
            }
        }
    }

    public void SetRimTransform(Transform rim)
    {
        rimTransform = rim;
    }

    public void ApplyDefaultPose()
    {
        transform.position = defaultRootPosition;
        if (rimTransform != null)
        {
            rimTransform.localPosition = defaultRimLocalPosition;
        }
    }

    public void RandomizePose(int curriculumStage)
    {
        float stageScale = ArcAcademyLayout.GetStageHoopOffsetScale(curriculumStage);
        float offsetX = Random.Range(-ArcAcademyLayout.MaxHoopOffsetX, ArcAcademyLayout.MaxHoopOffsetX) * stageScale;
        float offsetZ = Random.Range(-ArcAcademyLayout.MaxHoopOffsetZ, ArcAcademyLayout.MaxHoopOffsetZ) * stageScale;
        transform.position = defaultRootPosition + new Vector3(offsetX, 0f, offsetZ);

        if (rimTransform == null)
        {
            return;
        }

        float height = Mathf.Lerp(
            ArcAcademyLayout.MinRimHeight,
            ArcAcademyLayout.MaxRimHeight,
            Random.Range(0f, stageScale));
        rimTransform.localPosition = new Vector3(
            defaultRimLocalPosition.x,
            height,
            defaultRimLocalPosition.z);
    }
}

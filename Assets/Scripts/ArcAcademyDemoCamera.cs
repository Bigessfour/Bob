using System.Collections;
using UnityEngine;

/// <summary>
/// Hero, orbit, and free-fly camera for Arc Academy demo navigation.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ArcAcademyDemoCamera : MonoBehaviour
{
    public enum CameraMode
    {
        Hero,
        LabHero,
        Orbit,
        FreeFly,
    }

    [SerializeField] private float orbitDistance = 14f;
    [SerializeField] private float orbitMinPitch = 5f;
    [SerializeField] private float orbitMaxPitch = 75f;
    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float lookSensitivity = 2.5f;

    private CameraMode mode = CameraMode.Hero;
    private float orbitYaw;
    private float orbitPitch = 18f;
    private float flyYaw;
    private float flyPitch = 15f;
    private Coroutine followRoutine;

    public static ArcAcademyDemoCamera Instance { get; private set; }

    public CameraMode Mode => mode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        if (SimpleArcAcademyArena.IsLabViewActive)
        {
            ResetToLabHero();
        }
        else
        {
            ResetToHero();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (Application.isPlaying && SimpleArcAcademyArena.IsLabViewActive)
        {
            ResetToLabHero();
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            CycleMode();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (SimpleArcAcademyArena.IsLabViewActive)
            {
                ToggleLabTrainingCameras();
            }
            else
            {
                ResetToHero();
            }
        }

        switch (mode)
        {
            case CameraMode.Orbit:
                UpdateOrbit();
                break;
            case CameraMode.FreeFly:
                UpdateFreeFly();
                break;
        }
    }

    public void CycleMode()
    {
        mode = mode switch
        {
            CameraMode.LabHero => CameraMode.Hero,
            CameraMode.Hero => CameraMode.Orbit,
            CameraMode.Orbit => CameraMode.FreeFly,
            _ => SimpleArcAcademyArena.IsLabViewActive ? CameraMode.LabHero : CameraMode.Hero,
        };

        if (mode == CameraMode.Hero)
        {
            ResetToHero();
        }
        else if (mode == CameraMode.LabHero)
        {
            ResetToLabHero();
        }
        else if (mode == CameraMode.Orbit)
        {
            Vector3 orbitLookAt = SimpleArcAcademyArena.IsLabViewActive
                ? SimpleArcAcademyArena.LabCameraLookAt
                : ArcAcademyLayout.CameraLookAt;
            Vector3 toCam = transform.position - orbitLookAt;
            orbitDistance = toCam.magnitude;
            orbitYaw = Mathf.Atan2(toCam.x, toCam.z) * Mathf.Rad2Deg;
            orbitPitch = Mathf.Asin(toCam.y / Mathf.Max(0.01f, orbitDistance)) * Mathf.Rad2Deg;
        }
        else
        {
            flyYaw = transform.eulerAngles.y;
            flyPitch = transform.eulerAngles.x;
        }

        Debug.Log($"Arc Academy camera: {mode}");
    }

    public void ResetToLabHero()
    {
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
            followRoutine = null;
        }

        mode = CameraMode.LabHero;
        transform.position = SimpleArcAcademyArena.LabCameraPosition;
        transform.rotation = Quaternion.LookRotation(
            SimpleArcAcademyArena.LabCameraLookAt - SimpleArcAcademyArena.LabCameraPosition,
            Vector3.up);

        if (TryGetComponent(out Camera cam))
        {
            cam.fieldOfView = SimpleArcAcademyArena.LabCameraFieldOfView;
        }
    }

    public void ResetToHero()
    {
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
            followRoutine = null;
        }

        mode = CameraMode.Hero;
        Vector3 heroPosition = SimpleArcAcademyArena.GetHeroCameraPosition;
        Vector3 heroLookAt = SimpleArcAcademyArena.GetHeroCameraLookAt;
        transform.position = heroPosition;
        transform.rotation = Quaternion.LookRotation(heroLookAt - heroPosition, Vector3.up);

        if (TryGetComponent(out Camera cam))
        {
            cam.fieldOfView = SimpleArcAcademyArena.GetHeroCameraFieldOfView;
        }
    }

    private void ToggleLabTrainingCameras()
    {
        if (mode == CameraMode.LabHero)
        {
            ResetToHero();
        }
        else
        {
            ResetToLabHero();
        }
    }

    public void FollowEntrance(Transform target, float duration)
    {
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
        }

        followRoutine = StartCoroutine(FollowEntranceRoutine(target, duration));
    }

    private IEnumerator FollowEntranceRoutine(Transform target, float duration)
    {
        Vector3 startPos = ArcAcademyLayout.EntranceCameraPosition;
        Vector3 endPos = ArcAcademyLayout.CameraPosition;
        Quaternion startRot = Quaternion.LookRotation(
            ArcAcademyLayout.EntranceCameraLookAt - startPos, Vector3.up);
        Quaternion endRot = Quaternion.LookRotation(
            ArcAcademyLayout.CameraLookAt - endPos, Vector3.up);

        transform.position = startPos;
        transform.rotation = startRot;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            Vector3 lookTarget = target != null
                ? Vector3.Lerp(ArcAcademyLayout.EntranceCameraLookAt, target.position, t)
                : ArcAcademyLayout.CameraLookAt;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            yield return null;
        }

        ResetToHero();
        followRoutine = null;
    }

    private void UpdateOrbit()
    {
        if (Input.GetMouseButton(1))
        {
            orbitYaw += Input.GetAxis("Mouse X") * lookSensitivity;
            orbitPitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            orbitPitch = Mathf.Clamp(orbitPitch, orbitMinPitch, orbitMaxPitch);
        }

        orbitDistance = Mathf.Clamp(orbitDistance - Input.mouseScrollDelta.y * 0.8f, 4f, 28f);

        Vector3 orbitTarget = SimpleArcAcademyArena.IsLabViewActive
            ? SimpleArcAcademyArena.LabCameraLookAt
            : ArcAcademyLayout.CameraLookAt;

        Quaternion rot = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        transform.position = orbitTarget + rot * (Vector3.back * orbitDistance);
        transform.rotation = Quaternion.LookRotation(orbitTarget - transform.position, Vector3.up);
    }

    private void UpdateFreeFly()
    {
        if (Input.GetMouseButton(1))
        {
            flyYaw += Input.GetAxis("Mouse X") * lookSensitivity;
            flyPitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            flyPitch = Mathf.Clamp(flyPitch, -85f, 85f);
        }

        transform.rotation = Quaternion.Euler(flyPitch, flyYaw, 0f);
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            move += transform.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            move -= transform.forward;
        }

        if (Input.GetKey(KeyCode.A))
        {
            move -= transform.right;
        }

        if (Input.GetKey(KeyCode.D))
        {
            move += transform.right;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            move -= Vector3.up;
        }

        if (Input.GetKey(KeyCode.E))
        {
            move += Vector3.up;
        }

        transform.position += move.normalized * (flySpeed * Time.deltaTime);
    }
}

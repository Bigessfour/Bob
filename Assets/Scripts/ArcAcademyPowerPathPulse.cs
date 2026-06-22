using System.Collections;
using UnityEngine;

/// <summary>
/// Emissive floor pulse from Bob toward the hoop on shot release.
/// </summary>
public class ArcAcademyPowerPathPulse : MonoBehaviour
{
    public static ArcAcademyPowerPathPulse Instance { get; private set; }

    [SerializeField] private float pulseDuration = 0.6f;
    [SerializeField] private float lineWidth = 0.35f;
    [SerializeField] private float lineHeight = 0.04f;

    private LineRenderer lineRenderer;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        EnsureLineRenderer();
        BobPhysicsLayers.SetLayerRecursively(gameObject, BobPhysicsLayers.DecorationLayer);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlayPulse(Vector3 from, Vector3 to)
    {
        EnsureLineRenderer();
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }

        pulseRoutine = StartCoroutine(PulseRoutine(from, to));
    }

    private IEnumerator PulseRoutine(Vector3 from, Vector3 to)
    {
        var start = new Vector3(from.x, lineHeight, from.z);
        var end = new Vector3(to.x, lineHeight, to.z);
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start);

        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / pulseDuration);
            lineRenderer.SetPosition(1, Vector3.Lerp(start, end, t));

            var color = lineRenderer.startColor;
            color.a = Mathf.Lerp(0.85f, 0f, t);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            yield return null;
        }

        lineRenderer.enabled = false;
        pulseRoutine = null;
    }

    private void EnsureLineRenderer()
    {
        if (lineRenderer != null)
        {
            return;
        }

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth * 0.35f;
        lineRenderer.numCapVertices = 4;
        lineRenderer.enabled = false;
        var emissive = new Color(1f, 0.55f, 0.15f, 0.85f);
        lineRenderer.startColor = emissive;
        lineRenderer.endColor = emissive;
        lineRenderer.sharedMaterial = ArcAcademyShaderUtility.CreateEmissiveLineMaterial(emissive, 1.5f);
    }
}

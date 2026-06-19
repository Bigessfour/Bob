using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Bob's Arc Academy entrance — sky drop on first episode, quick pop on resets.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BobEntranceController : MonoBehaviour
{
    [SerializeField] private float introDropHeight = 4f;
    [SerializeField] private float introDuration = 1.15f;
    [SerializeField] private float resetDuration = 0.42f;
    [SerializeField] private float resetPopScale = 1.12f;

    private Rigidbody rb;
    private Renderer bodyRenderer;
    private Vector3 baseScale;
    private Coroutine activeRoutine;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

    public bool IsActive { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bodyRenderer = GetComponent<Renderer>();
        baseScale = transform.localScale;
    }

    public void PlaySessionIntro(Vector3 spawnPosition, Action onComplete)
    {
        StartSequence(SessionIntroRoutine(spawnPosition, onComplete));
    }

    public void PlayEpisodeReset(Vector3 spawnPosition, Action onComplete)
    {
        StartSequence(EpisodeResetRoutine(spawnPosition, onComplete));
    }

    private void StartSequence(IEnumerator routine)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(routine);
    }

    private IEnumerator SessionIntroRoutine(Vector3 spawnPosition, Action onComplete)
    {
        IsActive = true;
        SetPhysicsLocked(true);

        Vector3 start = spawnPosition + Vector3.up * introDropHeight;
        transform.position = start;
        transform.localScale = baseScale * 0.65f;
        SetAlpha(0.25f);

        ArcAcademyDemoCamera.Instance?.FollowEntrance(transform, introDuration);

        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / introDuration);
            transform.position = Vector3.Lerp(start, spawnPosition, t);
            transform.localScale = Vector3.Lerp(baseScale * 0.65f, baseScale, t);
            SetAlpha(Mathf.Lerp(0.25f, 1f, t));
            yield return null;
        }

        transform.position = spawnPosition;
        transform.localScale = baseScale;
        SetAlpha(1f);
        ArcAcademyManager.Instance?.TriggerSpawnReady();
        FinishEntrance(onComplete);
    }

    private IEnumerator EpisodeResetRoutine(Vector3 spawnPosition, Action onComplete)
    {
        IsActive = true;
        SetPhysicsLocked(true);

        transform.position = spawnPosition;
        float elapsed = 0f;
        while (elapsed < resetDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / resetDuration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * (resetPopScale - 1f);
            transform.localScale = baseScale * scale;
            yield return null;
        }

        transform.localScale = baseScale;
        ArcAcademyManager.Instance?.TriggerSpawnReady();
        FinishEntrance(onComplete);
    }

    private void FinishEntrance(Action onComplete)
    {
        StartCoroutine(ReleasePhysicsAndComplete(onComplete));
    }

    private IEnumerator ReleasePhysicsAndComplete(Action onComplete)
    {
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Unity 6 may still treat the body as kinematic until the next physics step.
        yield return new WaitForFixedUpdate();

        BobPhysicsUtility.ClearVelocitiesIfDynamic(rb);

        IsActive = false;
        activeRoutine = null;
        onComplete?.Invoke();
    }

    private void SetPhysicsLocked(bool locked)
    {
        if (rb == null)
        {
            return;
        }

        if (!locked)
        {
            return;
        }

        if (!rb.isKinematic)
        {
            BobPhysicsUtility.ClearVelocitiesIfDynamic(rb);
        }

        rb.isKinematic = true;
    }

    private void SetAlpha(float alpha)
    {
        if (bodyRenderer == null)
        {
            return;
        }

        var mat = bodyRenderer.material;
        if (mat.HasProperty(BaseColorId))
        {
            Color c = mat.GetColor(BaseColorId);
            c.a = alpha;
            mat.SetColor(BaseColorId, c);
        }

        if (mat.HasProperty(EmissiveColorId))
        {
            Color e = mat.GetColor(EmissiveColorId);
            mat.SetColor(EmissiveColorId, e * Mathf.Lerp(0.4f, 1f, alpha));
        }
    }
}

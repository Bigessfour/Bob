using UnityEngine;

/// <summary>
/// World-space score popup near the active hoop (swish vs made basket).
/// </summary>
public class ArcAcademyScorePopup : MonoBehaviour
{
    [SerializeField] private TextMesh textMesh;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float popScale = 1.4f;

    private float hideTimer;
    private Vector3 baseScale = Vector3.one;
    private Color baseColor = Color.white;

    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMesh>();
        }

        if (textMesh != null)
        {
            baseScale = textMesh.transform.localScale;
            baseColor = textMesh.color;
            textMesh.gameObject.SetActive(false);
        }
    }

    public void Show(bool swish, int totalMade)
    {
        if (textMesh == null)
        {
            return;
        }

        textMesh.gameObject.SetActive(true);
        textMesh.text = swish
            ? $"SWISH! +2.5  ({totalMade})"
            : $"SCORE! +2.0  ({totalMade})";
        textMesh.color = swish ? new Color(0.4f, 1f, 0.6f) : new Color(1f, 0.85f, 0.3f);
        textMesh.transform.localScale = baseScale * popScale;
        hideTimer = displayDuration;
    }

    private void Update()
    {
        if (textMesh == null || hideTimer <= 0f)
        {
            return;
        }

        hideTimer -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(hideTimer / displayDuration);
        textMesh.transform.localScale = Vector3.Lerp(baseScale * popScale, baseScale, t);

        if (hideTimer <= 0f)
        {
            textMesh.gameObject.SetActive(false);
            textMesh.color = baseColor;
        }
    }
}

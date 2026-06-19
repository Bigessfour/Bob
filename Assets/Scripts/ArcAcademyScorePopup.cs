using UnityEngine;

/// <summary>
/// World-space score popup near the active hoop (swish vs made basket).
/// </summary>
public class ArcAcademyScorePopup : MonoBehaviour
{
    [SerializeField] private TextMesh textMesh;
    [SerializeField] private float displayDuration = 1.6f;
    [SerializeField] private float popScale = 1.55f;
    [SerializeField] private float riseDistance = 0.35f;

    private float hideTimer;
    private Vector3 baseScale = Vector3.one;
    private Vector3 baseWorldPosition;
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
            baseWorldPosition = textMesh.transform.position;
            baseColor = textMesh.color;
            textMesh.gameObject.SetActive(false);
            textMesh.characterSize = 0.14f;
            textMesh.fontStyle = FontStyle.Bold;
        }
    }

    public void Show(bool swish, int totalMade)
    {
        if (textMesh == null)
        {
            return;
        }

        textMesh.gameObject.SetActive(true);
        baseWorldPosition = transform.position;
        textMesh.text = swish
            ? $"SWISH! +{ArcAcademyRewards.MadeWithSwish:0.0}  ({totalMade})"
            : $"SCORE! +{ArcAcademyRewards.MadeBasket:0.0}  ({totalMade})";
        textMesh.color = swish ? new Color(0.35f, 1f, 0.55f, 1f) : new Color(1f, 0.82f, 0.25f, 1f);
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
        textMesh.transform.position = baseWorldPosition + Vector3.up * (riseDistance * t);

        Color c = textMesh.color;
        c.a = Mathf.Lerp(1f, 0f, t);
        textMesh.color = c;

        if (hideTimer <= 0f)
        {
            textMesh.gameObject.SetActive(false);
            textMesh.color = baseColor;
            textMesh.transform.localScale = baseScale;
        }
    }
}

using UnityEngine;

/// <summary>
/// World-space encouragement bubble above Bob on made baskets (AI Warehouse style).
/// White rounded panel + orange name highlight + camera-facing billboard.
/// </summary>
public class BobSpeechBubble : MonoBehaviour
{
    [SerializeField] private TextMesh textMesh;
    [SerializeField] private Transform background;
    [SerializeField] private float displayDuration = 2.4f;
    [SerializeField] private float riseDistance = 0.35f;
    [SerializeField] private Vector3 localOffset = new(0f, 1.15f, 0f);

    private float hideTimer;
    private Vector3 baseLocalPosition;
    private Vector3 baseScale = Vector3.one;
    private Color baseTextColor = Color.white;

    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMesh>(true);
        }

        if (background == null)
        {
            var bg = transform.Find("BubbleBackground");
            if (bg != null)
            {
                background = bg;
            }
        }

        if (GetComponent<CameraFacingBillboard>() == null)
        {
            gameObject.AddComponent<CameraFacingBillboard>();
        }

        if (textMesh != null)
        {
            baseLocalPosition = textMesh.transform.localPosition;
            baseScale = textMesh.transform.localScale;
            baseTextColor = textMesh.color;
            textMesh.gameObject.SetActive(false);
            textMesh.richText = true;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.075f;
        }

        if (background != null)
        {
            background.gameObject.SetActive(false);
        }
    }

    public void Show(bool swish)
    {
        if (textMesh == null)
        {
            return;
        }

        string praise = swish ? "Swish! Great job, Bob!" : "Great job, Bob!";
        textMesh.text = BobVisualProfile.FormatPraise(praise);
        textMesh.color = baseTextColor;
        textMesh.gameObject.SetActive(true);

        if (background != null)
        {
            background.gameObject.SetActive(true);
        }

        textMesh.transform.localPosition = baseLocalPosition;
        textMesh.transform.localScale = baseScale;
        hideTimer = displayDuration;
    }

    private void LateUpdate()
    {
        if (textMesh == null || hideTimer <= 0f)
        {
            return;
        }

        hideTimer -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(hideTimer / displayDuration);
        textMesh.transform.localPosition = baseLocalPosition + Vector3.up * (riseDistance * t);

        Color c = textMesh.color;
        c.a = Mathf.Lerp(1f, 0f, t);
        textMesh.color = c;

        if (background != null)
        {
            var bgRenderer = background.GetComponent<Renderer>();
            if (bgRenderer != null && bgRenderer.material.HasProperty("_BaseColor"))
            {
                Color bg = bgRenderer.material.GetColor("_BaseColor");
                bg.a = c.a * 0.94f;
                bgRenderer.material.SetColor("_BaseColor", bg);
            }
        }

        if (hideTimer <= 0f)
        {
            textMesh.gameObject.SetActive(false);
            textMesh.color = baseTextColor;
            textMesh.transform.localPosition = baseLocalPosition;

            if (background != null)
            {
                background.gameObject.SetActive(false);
            }
        }
    }
}

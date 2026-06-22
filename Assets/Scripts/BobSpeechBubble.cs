using UnityEngine;

/// <summary>
/// World-space encouragement bubble above Bob on made baskets (AI Warehouse style).
/// </summary>
public class BobSpeechBubble : MonoBehaviour
{
    [SerializeField] private TextMesh textMesh;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float riseDistance = 0.45f;
    [SerializeField] private Vector3 localOffset = new(0f, 1.1f, 0f);

    private float hideTimer;
    private Vector3 baseLocalPosition;
    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMesh>(true);
        }

        if (textMesh != null)
        {
            baseLocalPosition = textMesh.transform.localPosition;
            baseScale = textMesh.transform.localScale;
            textMesh.gameObject.SetActive(false);
            textMesh.characterSize = 0.08f;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;
        }
    }

    public void Show(bool swish)
    {
        if (textMesh == null)
        {
            return;
        }

        textMesh.gameObject.SetActive(true);
        textMesh.text = swish ? "Swish! Great job, Bob!" : "Great job, Bob!";
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

        if (hideTimer <= 0f)
        {
            textMesh.gameObject.SetActive(false);
            textMesh.color = Color.white;
            textMesh.transform.localPosition = baseLocalPosition;
        }
    }
}

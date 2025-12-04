using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using CandyCoded.HapticFeedback;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ClickFall : MonoBehaviour, IPointerClickHandler
{
    [Header("Hiển thị (optional)")]
    public SpriteRenderer spriteRenderer;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public float fadeSpeed = 10f;
    public Color activeColor = new Color(0.2f, 0.8f, 0.2f, 1f);  // xanh lá

    [Header("Vật lý")]
    public float gravityScale = 1f;
    public bool addRigidbodyIfMissing = true;

    [Header("Highlight")]
    [Tooltip("Obj con chứa viền highlight, mặc định tắt")]
    public GameObject highlightObject;
    public float highlightTime = 0.15f;

    Collider2D  col;
    Rigidbody2D rb;

    bool hasFallen   = false;  // đã bắt đầu rơi chưa
    bool hasNotified = false;  // đã báo GameManager chưa

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        // Ban đầu: chỉ để click (trigger), chưa chịu gravity
        if (col != null)
            col.isTrigger = true;

        if (rb != null)
            rb.simulated = false;

        // đảm bảo highlight tắt lúc đầu
        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    // Cho test trên PC/mac không cần EventSystem
    void OnMouseDown()
    {
        HandleClick();   // input người chơi
    }

    // Cho mobile / EventSystem + Physics2DRaycaster
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();   // input người chơi
    }

    // ====== CLICK BÌNH THƯỜNG (người chơi) ======
    void HandleClick()
    {
        if (hasFallen) return;

        // Chỉ cho người chơi click khi đang Gameplay
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Gameplay)
        {
            return;
        }

        DoFall();
    }

    // ====== CLICK CHO DEMO (tay auto) ======
    public void DemoClick()
    {
        // Bỏ qua check state, dùng chung logic rơi
        DoFall();
    }

    // ====== LOGIC LÀM BLOCK RƠI (dùng chung) ======
    void DoFall()
    {
        if (hasFallen) return;
        hasFallen = true;

        AudioManager.Instance?.PlayClickBlock();

        HapticFeedback.LightFeedback();

        // tắt trigger để khối va chạm thật
        if (col != null)
            col.isTrigger = false;

        // hiệu ứng đổi alpha + chuyển màu xanh lá
        if (spriteRenderer != null)
            StartCoroutine(FadeToActiveColor());

        // highlight viền
        if (highlightObject != null)
            StartCoroutine(HighlightRoutine());

        // thêm Rigidbody nếu thiếu
        if (rb == null && addRigidbodyIfMissing)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // bật physics
        if (rb != null)
        {
            rb.simulated = true;
            rb.gravityScale = gravityScale;
        }

        // Báo cho GameManager: khối này đã bắt đầu rơi
        if (!hasNotified)
        {
            hasNotified = true;
            GameManager.Instance?.NotifyBlockFallen(this);
        }

        Debug.Log("[ClickFall] FALL: " + name);
    }

    IEnumerator FadeToActiveColor()
    {
        if (spriteRenderer == null) yield break;

        float t = 0f;
        Color startColor = spriteRenderer.color;
        float startAlpha = startColor.a;
        float targetAlpha = maxAlpha;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;

            float a = Mathf.Lerp(startAlpha, targetAlpha, t);
            Color c = Color.Lerp(startColor, activeColor, t);
            c.a = a;
            spriteRenderer.color = c;

            yield return null;
        }

        Color final = activeColor;
        final.a = targetAlpha;
        spriteRenderer.color = final;
    }

    IEnumerator HighlightRoutine()
    {
        // bật highlight
        highlightObject.SetActive(true);

        yield return new WaitForSeconds(highlightTime);

        // tắt highlight
        if (highlightObject != null)
            highlightObject.SetActive(false);
    }
}

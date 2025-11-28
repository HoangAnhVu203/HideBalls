using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ClickFall : MonoBehaviour, IPointerClickHandler
{
    [Header("Hiển thị (optional)")]
    public SpriteRenderer spriteRenderer;
    [Range(0f, 1f)] public float maxAlpha = 1f;   
    public float fadeSpeed = 10f;

    [Header("Vật lý")]
    public float gravityScale = 1f;
    public bool addRigidbodyIfMissing = true;

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
    }

    // Cho test trên PC/mac không cần EventSystem
    void OnMouseDown()
    {
        HandleClick();
    }

    // Cho mobile / UI EventSystem + Physics2DRaycaster
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    void HandleClick()
    {
        // nếu đã rơi rồi thì bỏ qua
        if (hasFallen) return;

        // Nếu có GameManager và game không ở Gameplay thì không cho click
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Gameplay)
        {
            return;
        }

        hasFallen = true;

        // tắt trigger để khối va chạm thật
        if (col != null)
            col.isTrigger = false;

        // hiệu ứng alpha (tuỳ bạn có dùng hay không)
        if (spriteRenderer != null)
            StartCoroutine(FadeToAlpha(spriteRenderer.color.a, maxAlpha));

        // thêm Rigidbody nếu thiếu
        if (rb == null && addRigidbodyIfMissing)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // bật physics
        if (rb != null)
        {
            rb.simulated   = true;
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

    IEnumerator FadeToAlpha(float from, float to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            float a = Mathf.Lerp(from, to, t);

            if (spriteRenderer != null)
            {
                var c = spriteRenderer.color;
                c.a = a;
                spriteRenderer.color = c;
            }

            yield return null;
        }
    }
}

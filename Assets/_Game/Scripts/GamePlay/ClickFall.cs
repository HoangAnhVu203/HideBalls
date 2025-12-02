using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ClickFall : MonoBehaviour, IPointerClickHandler
{
    [Header("Hiển thị")]
    public SpriteRenderer spriteRenderer;
    public Color activeColor = new Color(0.2f, 0.85f, 0.2f, 1f);   // xanh lá
    [Range(0f, 1f)] public float startAlpha = 0.3f;               // alpha ban đầu
    [Range(0f, 1f)] public float maxAlpha   = 1f;                 // alpha khi click
    public float fadeSpeed = 8f;

    [Header("Vật lý")]
    public float gravityScale = 1f;
    public bool addRigidbodyIfMissing = true;

    Collider2D col;
    Rigidbody2D rb;

    bool hasFallen   = false;
    bool hasNotified = false;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        // ========== TRẠNG THÁI BAN ĐẦU ==========
        if (col) col.isTrigger = true;
        if (rb)  rb.simulated = false;

        // set alpha mờ lúc đầu
        if (spriteRenderer)
        {
            var c = spriteRenderer.color;
            c.a = startAlpha;
            spriteRenderer.color = c;
        }
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    void OnMouseDown()
    {
        HandleClick();
    }
#endif

    // Mobile + EventSystem + Physics2DRaycaster
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    void HandleClick()
    {
        if (hasFallen) return;

        // chỉ cho click khi đang gameplay
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Gameplay)
            return;

        hasFallen = true;

        // cho va chạm vật lý thật
        if (col) col.isTrigger = false;

        // đổi màu sang xanh
        if (spriteRenderer)
        {
            spriteRenderer.color = new Color(activeColor.r, activeColor.g, activeColor.b, spriteRenderer.color.a);
            StartCoroutine(FadeToAlpha(spriteRenderer.color.a, maxAlpha));
        }

        // thêm Rigidbody
        if (!rb && addRigidbodyIfMissing)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // bật vật lý
        if (rb)
        {
            rb.simulated = true;
            rb.gravityScale = gravityScale;
        }

        // báo GameManager
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

            if (spriteRenderer)
            {
                var c = spriteRenderer.color;
                c.a = a;
                spriteRenderer.color = c;
            }

            yield return null;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickFall : MonoBehaviour, IPointerClickHandler
{
    [Header("ShowOut")]
    public SpriteRenderer spriteRenderer;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public float fadeSpeed = 10f;

    [Header("Physic")]
    public float gravityScale = 1f;
    public bool addRigidbodyIfMissing = true;

    [Header("Click 2 times")]
    public float jumpForce = 3f;     
    public float shrinkDuration = 0.3f; 
    public bool fadeOutOnShrink = true; 

    Collider2D col;
    Rigidbody2D rb;

    enum State
    {
        Idle,       
        Fallen,    
        Jumping,    
        Popping     
    }

    State state = State.Idle;
    bool destroyStarted = false;
    Vector3 originalScale;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        if (col)
            col.isTrigger = true;

        originalScale = transform.localScale;
    }

    void OnMouseDown()
    {
        HandleClick();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    void HandleClick()
    {
        switch (state)
        {
            case State.Idle:
                ActivateFall();   
                break;

            case State.Fallen:
                StartJump();     
                break;

            case State.Jumping:
            case State.Popping:
                break;
        }
    }

    void ActivateFall()
    {
        state = State.Fallen;

        if (col != null)
            col.isTrigger = false;

        if (spriteRenderer)
            StartCoroutine(FadeToAlpha(spriteRenderer.color.a, maxAlpha));
        if (!rb && addRigidbodyIfMissing)
            rb = gameObject.AddComponent<Rigidbody2D>();

        if (rb)
        {
            rb.simulated = true;
            rb.gravityScale = gravityScale;
        }

        Debug.Log("[ClickFall] Activated fall: " + name);
    }

    void StartJump()
    {
        if (state != State.Fallen || rb == null) return;

        state = State.Jumping;

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        Debug.Log("[ClickFall] Start jump: " + name);
    }

    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == State.Jumping && !destroyStarted)
        {
            destroyStarted = true;
            state = State.Popping;
            StartCoroutine(ShrinkAndDestroy());
        }
    }

    IEnumerator ShrinkAndDestroy()
    {

        if (col)
            col.enabled = false;

        float t = 0f;
        Vector3 startScale = transform.localScale;

        Color startColor = Color.white;
        if (spriteRenderer)
            startColor = spriteRenderer.color;

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkDuration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            if (fadeOutOnShrink && spriteRenderer)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
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

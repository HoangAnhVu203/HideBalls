using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ClickDeActive : MonoBehaviour, IPointerClickHandler
{
    [Header("Jump (one click only)")]
    public float jumpForce = 5f;
    public float delayBeforeDisappear = 0.1f;

    [Header("Ground")]
    [Tooltip("Layer được coi là mặt đất. Để None = va chạm với bất kỳ collider nào cũng tính là mặt đất.")]
    public LayerMask groundLayer;

    Rigidbody2D rb;
    Collider2D col;

    bool hasClicked = false;
    bool isJumping = false;
    bool isDisappearing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.simulated = true;
        rb.gravityScale = rb.gravityScale <= 0 ? 1f : rb.gravityScale;
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
        if (hasClicked) return;  // chỉ cho nhảy một lần

        hasClicked = true;
        isJumping = true;

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        Debug.Log("[ClickDeActive] Jump: " + name);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isJumping || isDisappearing) return;

        bool isGroundHit = false;

        if (groundLayer == 0)
        {
            // Không set layer → mọi va chạm đều coi là mặt đất
            isGroundHit = true;
        }
        else
        {
            int otherLayer = collision.collider.gameObject.layer;
            if (((1 << otherLayer) & groundLayer) != 0)
                isGroundHit = true;
        }

        if (isGroundHit)
        {
            Debug.Log("[ClickDeActive] Landed on ground, will disappear: " + name);
            StartDisappear();
        }
    }

    void StartDisappear()
    {
        if (isDisappearing) return;

        isDisappearing = true;
        StartCoroutine(DisappearRoutine());
    }

    IEnumerator DisappearRoutine()
    {
        if (col)
            col.enabled = false;

        yield return new WaitForSeconds(delayBeforeDisappear);

        Destroy(gameObject);
    }
}

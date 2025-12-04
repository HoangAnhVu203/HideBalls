using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using CandyCoded.HapticFeedback;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ClickDeActive : MonoBehaviour, IPointerClickHandler
{
    public float jumpForce = 5f;
    public float delayBeforeDisappear = 0.1f;

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
        AudioManager.Instance?.PlayClickBlock();

        HapticFeedback.LightFeedback();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    void HandleClick()
    {
        if (hasClicked) return; 

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

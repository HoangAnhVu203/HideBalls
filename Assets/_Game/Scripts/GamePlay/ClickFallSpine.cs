using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Spine;
using Spine.Unity;

[RequireComponent(typeof(Collider2D))]
public class ClickFallSpine : MonoBehaviour, IPointerClickHandler
{
    [Header("Spine")]
    public SkeletonAnimation skeletonAnimation;     

    [Header("Màu sắc")]
    public Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f); 
    public Color activeColor   = new Color(0.2f, 0.8f, 0.2f, 1f); 
    public float colorLerpSpeed = 10f;

    [Header("Vật lý")]
    public float gravityScale = 1f;
    public bool addRigidbodyIfMissing = true;

    [Header("Va chạm")]
    [Tooltip("Chọn layer Enemy (bọ đen, hazard...)")]
    public LayerMask enemyLayerMask;              

    Collider2D col;
    Rigidbody2D rb;
    bool clicked = false;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        // Tự tìm component Spine nếu chưa gán
        if (!skeletonAnimation)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        rb = GetComponent<Rigidbody2D>();

        // Ban đầu: chỉ để click, chưa va chạm vật lý
        if (col)
            col.isTrigger = true;

        // Set màu xám lúc đầu
        SetSpineColor(inactiveColor);

        // Tắt physics ban đầu
        if (rb)
        {
            rb.simulated = false;
            rb.gravityScale = 0f;
        }
    }

    // Dùng khi không có EventSystem 
    void OnMouseDown()
    {
        TryActivate();
    }

    // Dùng khi có EventSystem + Physics2DRaycaster
    public void OnPointerClick(PointerEventData eventData)
    {
        TryActivate();
    }

    void TryActivate()
    {
        if (clicked) return;
        clicked = true;

        // Tắt trigger để va chạm
        if (col != null)
            col.isTrigger = false;

        // Đổi màu xám -> xanh lá
        StopAllCoroutines();
        StartCoroutine(LerpSpineColor(inactiveColor, activeColor));


        if (!rb && addRigidbodyIfMissing)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // Bật physics
        if (rb)
        {
            rb.simulated = true;
            rb.gravityScale = gravityScale;
        }

        Debug.Log("[ClickFallSpine] Activated: " + name);
    }

    // ================== HÀM HỖ TRỢ SPINE ==================

    void SetSpineColor(Color c)
    {
        if (skeletonAnimation != null && skeletonAnimation.Skeleton != null)
        {
            skeletonAnimation.Skeleton.SetColor(c);
            skeletonAnimation.LateUpdate(); 
        }
    }

    IEnumerator LerpSpineColor(Color from, Color to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * colorLerpSpeed;
            Color c = Color.Lerp(from, to, t);
            SetSpineColor(c);
            yield return null;
        }

        SetSpineColor(to);
    }

    // ================== VA CHẠM VỚI ENEMY ==================

    // Nếu collider của block là non-trigger sau khi rơi
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsEnemyLayer(collision.collider.gameObject.layer))
        {
            // Đổi màu sang đen khi chạm enemy
            StopAllCoroutines();
            SetSpineColor(Color.black);
            Debug.Log("[ClickFallSpine] Hit Enemy (Collision): " + collision.collider.name);
        }
    }

    // dùng trigger cho enemy
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsEnemyLayer(other.gameObject.layer))
        {
            StopAllCoroutines();
            SetSpineColor(Color.black);
            Debug.Log("[ClickFallSpine] Hit Enemy (Trigger): " + other.name);
        }
    }

    bool IsEnemyLayer(int layer)
    {
        return ((1 << layer) & enemyLayerMask) != 0;
    }
}

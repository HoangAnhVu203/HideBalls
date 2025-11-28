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
    public Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f);  // xám lúc đầu
    public Color activeColor   = new Color(0.2f, 0.8f, 0.2f, 1f);  // xanh lá khi click
    public Color enemyHitColor = Color.black;                    // ĐEN khi trúng enemy
    public float colorLerpSpeed = 10f;

    [Header("Vật lý")]
    public float gravityScale = 1f;
    public bool addRigidbodyIfMissing = true;

    [Header("Va chạm")]
    [Tooltip("Layer Enemy (bọ đen, hazard...)")]
    public LayerMask enemyLayerMask;              

    Collider2D col;
    Rigidbody2D rb;
    bool clicked = false;
    bool hasHitEnemy = false;    // tránh trigger nhiều lần

    void Awake()
    {
        col = GetComponent<Collider2D>();

        // tự tìm Spine
        if (!skeletonAnimation)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        rb = GetComponent<Rigidbody2D>();

        // Ban đầu: chỉ click, chưa physics
        if (col)
            col.isTrigger = true;

        // Màu xám ban đầu
        SetSpineColor(inactiveColor);

        // Tắt physics ban đầu
        if (rb)
        {
            rb.simulated = false;
            rb.gravityScale = 0f;
        }
    }

    // TEST PC
    void OnMouseDown()
    {
        TryActivate();
    }

    // MOBILE / EventSystem
    public void OnPointerClick(PointerEventData eventData)
    {
        TryActivate();
    }

    // ================== CLICK ĐỂ RƠI ==================
    void TryActivate()
    {
        if (clicked) return;
        clicked = true;

        // bật collider thật
        if (col != null)
            col.isTrigger = false;

        // đổi màu xám -> xanh
        StopAllCoroutines();
        StartCoroutine(LerpSpineColor(inactiveColor, activeColor));

        // add rigid nếu thiếu
        if (!rb && addRigidbodyIfMissing)
            rb = gameObject.AddComponent<Rigidbody2D>();

        // bật physics
        if (rb)
        {
            rb.simulated = true;
            rb.gravityScale = gravityScale;
        }

        Debug.Log("[ClickFallSpine] Activated: " + name);
    }

    // ================== BALL VA CHẠM ENEMY => THUA ==================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsEnemyLayer(collision.collider.gameObject.layer))
        {
            HandleHitEnemy(collision.collider.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsEnemyLayer(other.gameObject.layer))
        {
            HandleHitEnemy(other.gameObject);
        }
    }

    void HandleHitEnemy(GameObject enemy)
    {
        if (hasHitEnemy) return;
        hasHitEnemy = true;

        // đổi màu sang đen
        StopAllCoroutines();
        SetSpineColor(enemyHitColor);

        Debug.Log("[ClickFallSpine] Hit Enemy: " + enemy.name);

        // báo GameManager xử thua
        GameManager.Instance?.OnBallHitEnemy();
    }

    bool IsEnemyLayer(int layer)
    {
        return ((1 << layer) & enemyLayerMask) != 0;
    }

    // ================== SPINE COLOR ==================
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
}

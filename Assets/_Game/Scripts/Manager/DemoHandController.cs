using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DemoHandClicker : MonoBehaviour
{
    [System.Serializable]
    public class DemoStep
    {
        [Tooltip("Điểm mà tay sẽ di chuyển tới")]
        public Transform handPoint;

        [Tooltip("Block gắn ClickFall")]
        public ClickFall clickFall;

        [Tooltip("Block gắn ClickFallSpine")]
        public ClickFallSpine clickFallSpine;

        [Tooltip("Chờ sau khi click (giây)")]
        public float waitAfterClick = 0.4f;
    }

    [Header("Tay demo")]
    public Transform handTransform;   // để trống sẽ dùng object này
    public float moveSpeed = 5f;
    public float stopDistance = 0.05f;

    [Header("Hiệu ứng click (scale)")]
    public float clickScaleDown = 0.85f;  // scale khi ấn
    public float clickSpeed = 15f;

    [Header("Chuỗi demo")]
    public DemoStep[] steps;

    Vector3 originalScale;

    void Awake()
    {
        if (!handTransform)
            handTransform = transform;

        originalScale = handTransform.localScale;
    }

    IEnumerator Start()
    {
        // Đợi GameManager sẵn sàng
        while (GameManager.Instance == null)
            yield return null;

        // Chỉ chạy khi đang ở Demo
        if (GameManager.Instance.CurrentState != GameManager.GameState.Demo)
            yield break;

        if (steps == null || steps.Length == 0)
        {
            Debug.LogWarning("[DemoHandClicker] Chưa cấu hình steps.");
            yield break;
        }

        // Đặt tay tại vị trí bước đầu
        if (steps[0].handPoint != null)
            handTransform.position = steps[0].handPoint.position;

        foreach (var step in steps)
        {
            if (GameManager.Instance.CurrentState != GameManager.GameState.Demo)
                yield break;

            // 1. Di chuyển
            if (step.handPoint != null)
                yield return MoveHandTo(step.handPoint.position);

            // 2. Hiệu ứng nhấn
            yield return ClickScaleEffect();

            // 3. Click object
            if (step.clickFall != null)
                step.clickFall.DemoClick();

            if (step.clickFallSpine != null)
                step.clickFallSpine.DemoClick();

            // 4. Delay
            yield return new WaitForSeconds(step.waitAfterClick);
        }
    }

    // ================= MOVE =================

    IEnumerator MoveHandTo(Vector3 targetPos)
    {
        while (Vector3.Distance(handTransform.position, targetPos) > stopDistance)
        {
            if (GameManager.Instance.CurrentState != GameManager.GameState.Demo)
                yield break;

            handTransform.position = Vector3.MoveTowards(
                handTransform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }
    }

    // ================= CLICK EFFECT =================

    IEnumerator ClickScaleEffect()
    {
        // Thu nhỏ
        Vector3 downScale = originalScale * clickScaleDown;
        yield return LerpScale(originalScale, downScale);

        // Phóng lại
        yield return LerpScale(downScale, originalScale);
    }

    IEnumerator LerpScale(Vector3 from, Vector3 to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * clickSpeed;
            handTransform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        handTransform.localScale = to;
    }
}

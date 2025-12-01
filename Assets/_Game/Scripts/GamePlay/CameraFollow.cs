using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target (ưu tiên kéo tay)")]
    public Transform target;                 // Kéo Ball vào đây cho chắc

    [Header("Tự tìm theo Layer (dự phòng)")]
    public string ballLayerName = "Ball";    // Tên layer của Ball

    [Header("Offset")]
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Dead Zone (vùng an toàn quanh tâm camera)")]
    public float deadZoneWidth = 3f;
    public float deadZoneHeight = 3f;

    [Header("Smooth")]
    public float smoothSpeed = 8f;

    [Header("Giới hạn camera (tuỳ chọn)")]
    public bool useClamp = false;
    public Vector2 minPos;
    public Vector2 maxPos;

    int ballLayer = -1;

    void Awake()
    {
        ballLayer = LayerMask.NameToLayer(ballLayerName);
        if (ballLayer == -1)
        {
            Debug.LogWarning("[CameraSmartFollowBall] Không tìm thấy layer: " + ballLayerName);
        }
    }

    void LateUpdate()
    {
        // Nếu chưa có target (chưa kéo tay hoặc ball mới spawn) -> tự tìm
        if (target == null)
        {
            FindBallTarget();
            if (target == null) return;  // vẫn chưa có thì thôi
        }

        // ====== LOGIC DEAD ZONE ======
        Vector3 camPos = transform.position;

        // Vị trí ball + offset (giữ Z của camera)
        Vector3 targetPos = target.position + offset;
        targetPos.z = camPos.z;

        float dx = targetPos.x - camPos.x;
        float dy = targetPos.y - camPos.y;

        float halfDeadW = deadZoneWidth * 0.5f;
        float halfDeadH = deadZoneHeight * 0.5f;

        // Nếu ball vượt ra ngoài vùng an toàn theo X
        if (Mathf.Abs(dx) > halfDeadW)
        {
            float moveX = dx - Mathf.Sign(dx) * halfDeadW;
            camPos.x += moveX;
        }

        // Nếu muốn camera KHÔNG follow theo Y, comment đoạn dưới:
        if (Mathf.Abs(dy) > halfDeadH)
        {
            float moveY = dy - Mathf.Sign(dy) * halfDeadH;
            camPos.y += moveY;
        }

        // Giới hạn trong map
        if (useClamp)
        {
            camPos.x = Mathf.Clamp(camPos.x, minPos.x, maxPos.x);
            camPos.y = Mathf.Clamp(camPos.y, minPos.y, maxPos.y);
        }

        // Lerp cho mượt
        transform.position = Vector3.Lerp(transform.position, camPos, smoothSpeed * Time.deltaTime);
    }

    void FindBallTarget()
    {
        // Nếu bạn đã kéo target bằng tay → bỏ qua
        if (target != null) return;

        // Tự tìm theo layer Ball
        if (ballLayer == -1) return;

        GameObject[] all = FindObjectsOfType<GameObject>();
        foreach (var go in all)
        {
            if (!go || !go.activeInHierarchy) continue;
            if (go.layer == ballLayer)
            {
                target = go.transform;
                Debug.Log("[CameraSmartFollowBall] Found ball by layer: " + go.name);
                return;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Vẽ vùng vàng để debug
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? transform.position : transform.position;
        center.z = 0;
        Gizmos.DrawWireCube(center, new Vector3(deadZoneWidth, deadZoneHeight, 0));
    }
#endif
}

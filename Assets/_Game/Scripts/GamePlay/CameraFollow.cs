using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollow : MonoBehaviour
{
    [Header("Target (Ball)")]
    public Transform target;                
    public string ballLayerName = "Ball";

    [Header("Offset khi FOLLOW ball")]
    public Vector3 followOffset = new Vector3(0, 0, -10);

    [Header("Vị trí camera MỚI mỗi lần LoadLevel")]
    public Vector3 startPosition = new Vector3(0, 0, -10);

    [Header("Dead Zone")]
    public float deadZoneWidth = 3.5f;
    public float deadZoneHeight = 10f;

    [Header("Smooth")]
    public float smoothSpeed = 5f;

    [Header("Clamp (optional)")]
    public bool useClamp = false;
    public Vector2 minPos;
    public Vector2 maxPos;

    [Header("Chỉ 1 số level mới follow")]
    public bool limitToSpecificLevels = false;
    public int[] followLevelIndices;   // ví dụ: 29, 31, 53,...

    int ballLayer = -1;
    int lastLevelIndex = int.MinValue;   // để phát hiện level mới

    void Awake()
    {
        ballLayer = LayerMask.NameToLayer(ballLayerName);
        if (ballLayer == -1)
            Debug.LogWarning("[CameraFollow] Không tìm thấy layer: " + ballLayerName);

        // Đảm bảo lúc bắt đầu đã ở đúng vị trí
        ResetCameraForNewLevel();
    }

    void LateUpdate()
    {
        // 1) PHÁT HIỆN LOAD LEVEL MỚI → RESET CAMERA
        if (LevelManager.Instance != null)
        {
            int curIndex = LevelManager.Instance.CurrentIndex;
            if (curIndex != lastLevelIndex)
            {
                lastLevelIndex = curIndex;
                ResetCameraForNewLevel();
            }
        }

        // 2) Nếu level này không cho follow -> đứng yên tại startPosition
        if (!CanFollowCurrentLevel())
            return;

        // 3) Follow ball nếu được phép
        if (target == null)
        {
            FindBallTarget();
            if (target == null) return;
        }

        Vector3 camPos = transform.position;

        Vector3 targetPos = target.position + followOffset;
        targetPos.z = camPos.z;

        float dx = targetPos.x - camPos.x;
        float dy = targetPos.y - camPos.y;

        float halfDeadW = deadZoneWidth * 0.5f;
        float halfDeadH = deadZoneHeight * 0.5f;

        // chỉ di chuyển nếu ball ra khỏi dead zone
        if (Mathf.Abs(dx) > halfDeadW)
            camPos.x += dx - Mathf.Sign(dx) * halfDeadW;

        if (Mathf.Abs(dy) > halfDeadH)
            camPos.y += dy - Mathf.Sign(dy) * halfDeadH;

        if (useClamp)
        {
            camPos.x = Mathf.Clamp(camPos.x, minPos.x, maxPos.x);
            camPos.y = Mathf.Clamp(camPos.y, minPos.y, maxPos.y);
        }

        transform.position = Vector3.Lerp(transform.position, camPos, smoothSpeed * Time.deltaTime);
    }

    // ----------------- HELPER -----------------

    void ResetCameraForNewLevel()
    {
        // mỗi lần level đổi: camera về vị trí start + clear target
        transform.position = startPosition;
        target = null;
        // Debug
        Debug.Log("[CameraFollow] Reset camera to " + startPosition + " (level changed)");
    }

    bool CanFollowCurrentLevel()
    {
        if (!limitToSpecificLevels) return true;

        if (LevelManager.Instance == null ||
            followLevelIndices == null ||
            followLevelIndices.Length == 0)
            return false;

        int idx = LevelManager.Instance.CurrentIndex;
        return followLevelIndices.Contains(idx);
    }

    void FindBallTarget()
    {
        if (ballLayer == -1) return;

        GameObject[] all = FindObjectsOfType<GameObject>();
        foreach (var go in all)
        {
            if (!go || !go.activeInHierarchy) continue;
            if (go.layer == ballLayer)
            {
                target = go.transform;
                Debug.Log("[CameraFollow] Found ball by layer: " + ballLayerName);
                return;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        center.z = 0;
        Gizmos.DrawWireCube(center, new Vector3(deadZoneWidth, deadZoneHeight, 0));
    }
#endif
}

using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        public string id;          // ID level (dùng cho LoadLevelById)
        public GameObject prefab;  // Prefab level
    }

    public static LevelManager Instance { get; private set; }

    [Header("Danh sách level (kéo prefab vào đây)")]
    public List<LevelEntry> levels = new List<LevelEntry>();

    [Header("Nơi spawn level (để trống = dùng chính GameObject này)")]
    public Transform levelRoot;

    [Header("Lưu tiến trình")]
    public bool saveProgress      = true;
    public bool loopAtEnd         = true;
    public int  defaultStartIndex = 0;       

    public const string PP_LEVEL_INDEX = "HIDEBALL_LEVEL";

    public int        CurrentIndex   { get; private set; } = -1;
    public GameObject CurrentLevelGO { get; private set; }

    // ================== RUNTIME ROOT CHO VFX / LASER ==================
    [Header("Runtime Root (parent cho VFX, debris, laser explosion, ...)")]
    public Transform runtimeRoot;    // Laser sẽ gán explosion/smoke vào đây

    // GameManager sẽ subscribe vào đây
    public event Action<GameObject, int> OnLevelLoaded;
    public event Action<int>             OnLevelUnloaded;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!levelRoot)
            levelRoot = transform;

        // Tạo runtimeRoot nếu chưa có
        if (!runtimeRoot)
        {
            GameObject rt = new GameObject("__RuntimeRoot");
            runtimeRoot = rt.transform;
            runtimeRoot.SetParent(transform);
            runtimeRoot.localPosition = Vector3.zero;
        }
    }

    void Start()
    {
        if (levels.Count == 0)
        {
            Debug.LogError("[LevelManager] Chưa có level nào trong danh sách!");
            return;
        }

        int startIndex = defaultStartIndex;

        if (saveProgress)
        {
            if (PlayerPrefs.HasKey(PP_LEVEL_INDEX))
            {
                startIndex = PlayerPrefs.GetInt(PP_LEVEL_INDEX, defaultStartIndex);
            }
        }
        else
        {
            // Không dùng saveProgress → coi như reset về demo
            PlayerPrefs.DeleteKey(PP_LEVEL_INDEX);
            PlayerPrefs.Save();
            startIndex = 0;   // luôn bắt đầu từ level demo
        }

        startIndex = Mathf.Clamp(startIndex, 0, levels.Count - 1);
        LoadLevel(startIndex);
    }


    // ================== PUBLIC API ==================

    public void Replay()
    {
        if (CurrentIndex >= 0)
            LoadLevel(CurrentIndex);
    }

    public void NextLevel()
    {
        if (levels.Count == 0) return;

        int next = CurrentIndex + 1;

        if (next >= levels.Count)
        {
            if (loopAtEnd)
            {
                next = defaultStartIndex;
                LoadLevel(next);
            }
            else
            {
                UIManager.Instance.OpenUI<PanelEndGame>();
            }
            return;
        }

        LoadLevel(next);
    }

    public void LoadLevelById(string id)
    {
        int index = levels.FindIndex(l => l.id == id);
        if (index >= 0)
        {
            LoadLevel(index);
        }
        else
        {
            Debug.LogWarning($"[LevelManager] Không tìm thấy level có id = {id}");
        }
    }

    // ================== CORE LOAD ==================

    public void LoadLevel(int index)
    {
        if (levels.Count == 0)
        {
            Debug.LogError("[LevelManager] Chưa có level nào trong danh sách!");
            return;
        }

        index = Mathf.Clamp(index, 0, levels.Count - 1);

        // 0. Clear mọi VFX runtime (laser explosion, smoke, debris, ...)
        ClearRuntime();

        // 1. Huỷ level cũ
        if (CurrentLevelGO != null)
        {
            OnLevelUnloaded?.Invoke(CurrentIndex);
            Destroy(CurrentLevelGO);
            CurrentLevelGO = null;
        }

        // 2. Spawn level mới
        LevelEntry entry = levels[index];

        if (entry.prefab == null)
        {
            Debug.LogError($"[LevelManager] Prefab rỗng tại level index {index}");
            return;
        }

        CurrentLevelGO = Instantiate(entry.prefab, levelRoot);
        CurrentLevelGO.name = string.IsNullOrEmpty(entry.id)
            ? $"Level_{index}"
            : entry.id;

        CurrentIndex = index;

        // 3. Lưu tiến trình
        if (saveProgress)
        {
            PlayerPrefs.SetInt(PP_LEVEL_INDEX, CurrentIndex);
            PlayerPrefs.Save();
        }

        // 4. Thông báo cho GameManager & các hệ thống khác
        OnLevelLoaded?.Invoke(CurrentLevelGO, CurrentIndex);

        Debug.Log($"[LevelManager] Loaded level index: {CurrentIndex}");

        HintSystem.Instance?.HideHint();
    }

    // ================== RUNTIME CLEANUP ==================

    /// <summary>
    /// Huỷ toàn bộ object con của runtimeRoot (VFX tạm, explosion, smoke,...)
    /// </summary>
    public void ClearRuntime()
    {
        if (runtimeRoot)
        {
            for (int i = runtimeRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(runtimeRoot.GetChild(i).gameObject);
            }
        }

        // (tuỳ chọn) Nếu sau này bạn còn VFX không parent vào runtimeRoot
        // có thể lọc theo tag/name tại đây.
    }
}

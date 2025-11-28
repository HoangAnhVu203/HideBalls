using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        public string id;         
        public GameObject prefab;  
    }

    public static LevelManager Instance { get; private set; }

    [Header("Danh sách level (kéo prefab vào đây)")]
    public List<LevelEntry> levels = new List<LevelEntry>();

    [Header("Nơi spawn level (để trống = dùng chính GameObject này)")]
    public Transform levelRoot;

    [Header("Lưu tiến trình")]
    public bool saveProgress   = true;
    public bool loopAtEnd      = true;
    public int  defaultStartIndex = 0;       // level bắt đầu khi chưa có save

    public const string PP_LEVEL_INDEX = "HIDEBALL_LEVEL";

    public int         CurrentIndex  { get; private set; } = -1;
    public GameObject  CurrentLevelGO { get; private set; }

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
    }

    void Start()
    {
        if (levels.Count == 0)
        {
            Debug.LogError("[LevelManager] Chưa có level nào trong danh sách!");
            return;
        }

        // Xác định level bắt đầu
        int startIndex = defaultStartIndex;

        if (saveProgress && PlayerPrefs.HasKey(PP_LEVEL_INDEX))
        {
            startIndex = PlayerPrefs.GetInt(PP_LEVEL_INDEX, defaultStartIndex);
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
            }
            else
            {
                Debug.Log("[LevelManager] Đã tới level cuối, không load tiếp.");
                return;
            }
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
    }
}

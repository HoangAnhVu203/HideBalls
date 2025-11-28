using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ================== STATE ==================
    public enum GameState
    {
        Gameplay,
        Pause,
        Win,
        Fail
    }

    public GameState CurrentState { get; private set; } = GameState.Gameplay;

    // ================== ENEMY / RAIN ==================
    [Header("Enemy Spawner")]
    [Tooltip("Component EnemySpawner dùng để mưa enemy")]
    public EnemySpawner enemySpawner;          // script EnemySpawner
    [Tooltip("GameObject chứa EnemySpawner, sẽ bật / tắt khi mưa")]
    public GameObject enemySpawnerGO;          // object bật tắt
    [Tooltip("Parent để chứa tất cả enemy spawn ra (để dễ clear)")]
    public Transform enemyRoot;                // parent enemies

    [Header("Rain Settings")]
    [Tooltip("Thời gian chờ thêm sau khi mưa dừng rồi mới xét WIN")]
    public float surviveExtraTime = 3f;

    int totalBlocks;       // tổng số ClickFall trong level
    int fallenBlocks;      // số block đã click rơi
    bool ballHit = false;  // ball đã từng trúng enemy chưa
    bool rainFinished = false;

    // ================== UNITY LIFECYCLE ==================
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        
        // nghe sự kiện load level từ LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded += OnLevelLoaded;

            // nếu LevelManager đã load level trước đó, setup lại 1 lần
            if (LevelManager.Instance.CurrentLevelGO != null)
            {
                SetupLevel(LevelManager.Instance.CurrentLevelGO);
            }
        }

        // đảm bảo spawner tắt khi mới vào game
        if (enemySpawnerGO != null)
            enemySpawnerGO.SetActive(false);
    }

    void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded -= OnLevelLoaded;
        }
    }

    // ================== LEVEL SETUP ==================

    // được gọi mỗi khi LevelManager load level mới
    void OnLevelLoaded(GameObject levelRoot, int index)
    {
        SetupLevel(levelRoot);
    }

    void SetupLevel(GameObject levelRoot)
    {
        ballHit = false;
        rainFinished = false;

        if (enemySpawner != null)
            enemySpawner.ResetSpawner();

        if (enemySpawnerGO != null)
            enemySpawnerGO.SetActive(false);

        ClearAllEnemies();

        ClickFall[] blocks = levelRoot.GetComponentsInChildren<ClickFall>(true);
        totalBlocks  = blocks.Length;
        fallenBlocks = 0;

        Debug.Log($"[GameManager] Setup level – blocks: {totalBlocks}");

        SetState(GameState.Gameplay);
        ShowCurrentLevel();

        if (totalBlocks == 0)
            StartRain();
    }


    void ClearAllEnemies()
    {
        if (enemyRoot != null)
        {
            for (int i = enemyRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(enemyRoot.GetChild(i).gameObject);
            }
        }
        else
        {
            // fallback: xoá theo tag
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
                Destroy(e);
        }
    }

    // ================== BLOCK EVENTS ==================

    /// <summary>
    /// Gọi từ ClickFall khi block bắt đầu rơi lần đầu tiên.
    /// </summary>
    public void NotifyBlockFallen(ClickFall block)
    {
        if (CurrentState != GameState.Gameplay) return;

        fallenBlocks++;
        Debug.Log($"[GameManager] Block fallen {fallenBlocks}/{totalBlocks}");

        if (fallenBlocks >= totalBlocks)
        {
            // Tất cả khối đã rơi -> bắt đầu mưa
            StartRain();
        }
    }

    // ================== RAIN CONTROL ==================

    void StartRain()
    {
        if (enemySpawnerGO == null || enemySpawner == null)
        {
            Debug.LogWarning("[GameManager] Chưa gán EnemySpawner hoặc enemySpawnerGO");
            return;
        }

        // bật object spawner và bắt đầu mưa
        enemySpawnerGO.SetActive(true);
        enemySpawner.StartRain();

        Debug.Log("[GameManager] Start rain");
    }

    /// <summary>
    /// Gọi từ EnemySpawner khi coroutine mưa kết thúc.
    /// </summary>
    public void OnRainFinished()
    {
        Debug.Log("[GameManager] Rain finished");
        rainFinished = true;

        // nếu ball chưa bị chạm -> chờ thêm rồi xét WIN
        if (!ballHit && CurrentState == GameState.Gameplay)
        {
            StartCoroutine(WaitAndCheckWin());
        }
    }

    IEnumerator WaitAndCheckWin()
    {
        yield return new WaitForSeconds(surviveExtraTime);

        if (!ballHit && rainFinished &&
            CurrentState == GameState.Gameplay)
        {
            SetState(GameState.Win);
        }
    }

    // ================== BALL HIT ENEMY ==================

    /// <summary>
    /// Gọi từ Ball (ClickFallSpine / BallSpine) khi va chạm enemy.
    /// </summary>
    public void OnBallHitEnemy()
    {
        if (CurrentState == GameState.Win || CurrentState == GameState.Fail)
            return;

        ballHit = true;
        SetState(GameState.Fail);
    }

    // ================== STATE & UI ==================

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("[GameManager] State -> " + newState);

        switch (newState)
        {
            case GameState.Gameplay:
                Time.timeScale = 1f;
                UIManager.Instance.OpenUI<CanvasGamePlay>();
                break;

            case GameState.Pause:
                Time.timeScale = 0f;
                break;

            case GameState.Win:
                Time.timeScale = 0f;
                UIManager.Instance.OpenUI<CanvasWin>();
                break;

            case GameState.Fail:
                Time.timeScale = 0f;
                UIManager.Instance.OpenUI<CanvasFail>();
                break;
        }
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    // ================== BUTTON HOẶC UI CALL ==================

    public void PauseGame()
    {
        if (CurrentState == GameState.Gameplay)
            SetState(GameState.Pause);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Pause)
            SetState(GameState.Gameplay);
    }

    public void RestartLevel()
    {
        LevelManager.Instance?.Replay();
    }

    public void NextLevel()
    {
        LevelManager.Instance?.NextLevel();
    }

    void ShowCurrentLevel()
    {
        if (LevelManager.Instance == null) return;

        var canvasUI = UIManager.Instance.GetUI<CanvasGamePlay>();
        if (canvasUI != null)
        {
            canvasUI.ShowLevel(LevelManager.Instance.CurrentIndex);
        }
    }
}

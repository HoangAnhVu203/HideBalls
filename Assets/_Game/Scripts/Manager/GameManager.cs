using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ================== STATE ==================
    public enum GameState
    {
        Demo,
        Gameplay,
        Pause,
        Win,
        Fail
    }

    public GameState CurrentState { get; private set; } = GameState.Gameplay;

    // ================== ENEMY / RAIN ==================
    [Header("Enemy Spawner")]
    [Tooltip("Component EnemySpawner dùng để mưa enemy")]
    public EnemySpawner enemySpawner;
    [Tooltip("GameObject chứa EnemySpawner, sẽ bật / tắt khi mưa")]
    public GameObject enemySpawnerGO;
    [Tooltip("Parent để chứa tất cả enemy spawn ra (để dễ clear)")]
    public Transform enemyRoot;

    [Header("Rain Settings")]
    [Tooltip("Thời gian chờ thêm sau khi mưa dừng rồi mới xét WIN / Replay demo")]
    public float surviveExtraTime = 3f;

    int totalBlocks;      // tổng số ClickFall trong level
    int fallenBlocks;     // số block đã click rơi
    bool ballHit      = false;  // ball đã từng trúng enemy chưa
    bool rainFinished = false;

    // ==== THUA KHI BÓNG RƠI QUÁ THẤP ====
    [Header("Lose when ball falls too low")]
    public bool enableFallLose = true;
    [Tooltip("Nếu y của bóng < loseY thì thua")]
    public float loseY = -5f;

    // danh sách bóng trong level hiện tại (tự tìm trong SetupLevel)
    Transform[] currentBalls;

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
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded += OnLevelLoaded;

            if (LevelManager.Instance.CurrentLevelGO != null)
            {
                SetupLevel(LevelManager.Instance.CurrentLevelGO);
            }
        }

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

    void Update()
    {
        // check rơi quá thấp chỉ áp dụng trong Gameplay thật
        if (enableFallLose && CurrentState == GameState.Gameplay)
        {
            CheckBallFallOut();
        }
    }

    // ================== LEVEL SETUP ==================

    void OnLevelLoaded(GameObject levelRoot, int index)
    {
        SetupLevel(levelRoot);
    }

    void SetupLevel(GameObject levelRoot)
    {
        ballHit      = false;
        rainFinished = false;

        if (enemySpawner != null)
            enemySpawner.ResetSpawner();

        if (enemySpawnerGO != null)
            enemySpawnerGO.SetActive(false);

        ClearAllEnemies();

        // Tìm tất cả ClickFall (block) trong level
        ClickFall[] blocks = levelRoot.GetComponentsInChildren<ClickFall>(true);
        totalBlocks  = blocks.Length;
        fallenBlocks = 0;

        // ==== TÌM TẤT CẢ BÓNG (ClickFallSpine) TRONG LEVEL ====
        var ballList = new System.Collections.Generic.List<Transform>();
        var spineBalls = levelRoot.GetComponentsInChildren<ClickFallSpine>(true);
        foreach (var b in spineBalls)
        {
            if (b != null)
                ballList.Add(b.transform);
        }
        currentBalls = ballList.ToArray();

        Debug.Log($"[GameManager] Setup level – blocks: {totalBlocks}, balls: {currentBalls.Length}");

        bool isDemoLevel = (LevelManager.Instance != null && LevelManager.Instance.CurrentIndex == 0);

        if (isDemoLevel)
        {
            SetState(GameState.Demo);
            UIManager.Instance.OpenUI<PanelDemo>();
        }
        else
        {
            SetState(GameState.Gameplay);
            ShowCurrentLevel();
            var canvas = UIManager.Instance.GetUI<CanvasGamePlay>();
            if (canvas != null)
            {
                canvas.ResetHintTimers();
            }
        }

        if (totalBlocks == 0)
            StartRain();
    }

    void ClearAllEnemies()
    {
        if (enemyRoot != null)
        {
            for (int i = enemyRoot.childCount - 1; i >= 0; i--)
                Destroy(enemyRoot.GetChild(i).gameObject);
        }
        else
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
                Destroy(e);
        }
    }

    // ================== BLOCK EVENTS ==================

    public void NotifyBlockFallen(ClickFall block)
    {
        if (CurrentState != GameState.Gameplay && CurrentState != GameState.Demo)
            return;

        fallenBlocks++;
        Debug.Log($"[GameManager] Block fallen {fallenBlocks}/{totalBlocks}");

        if (fallenBlocks >= totalBlocks)
        {
            StartRain();
        }
    }

    // ================== RAIN CONTROL ==================

    void StartRain()
    {
        if (enemySpawnerGO == null || enemySpawner == null)
        {
            return;
        }

        enemySpawnerGO.SetActive(true);
        enemySpawner.StartRain();

        Debug.Log("[GameManager] Start rain");
    }

    public void OnRainFinished()
    {
        Debug.Log("[GameManager] Rain finished");
        rainFinished = true;

        if (CurrentState == GameState.Demo)
        {
            StartCoroutine(DemoAutoReplay());
            return;
        }

        if (!ballHit && CurrentState == GameState.Gameplay)
        {
            StartCoroutine(WaitAndCheckWin());
        }
    }

    IEnumerator DemoAutoReplay()
    {
        yield return new WaitForSeconds(surviveExtraTime);

        if (CurrentState == GameState.Demo)
        {
            Debug.Log("[GameManager] Demo auto replay");
            RestartLevel();
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

    public void OnBallHitEnemy()
    {
        if (CurrentState == GameState.Demo)
            return;

        if (CurrentState == GameState.Win || CurrentState == GameState.Fail)
            return;

        ballHit = true;
        SetState(GameState.Fail);
    }

    // ==== THUA KHI BÓNG RƠI QUÁ THẤP ====
    void CheckBallFallOut()
    {
        if (currentBalls == null || currentBalls.Length == 0) return;

        foreach (var t in currentBalls)
        {
            if (t == null) continue;

            if (t.position.y < loseY)
            {
                Debug.Log("[GameManager] Ball fell below loseY => FAIL");
                SetState(GameState.Fail);
                return;
            }
        }
    }

    // ================== STATE & UI ==================

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("[GameManager] State -> " + newState);

        switch (newState)
        {
            case GameState.Demo:
                Time.timeScale = 1f;
                UIManager.Instance.OpenUI<PanelDemo>();
                break;

            case GameState.Gameplay:
                Time.timeScale = 1f;
                UIManager.Instance.OpenUI<CanvasGamePlay>();
                break;

            case GameState.Pause:
                Time.timeScale = 0f;
                break;

            case GameState.Win:
                Time.timeScale = 0f;
                AudioManager.Instance?.PlayWin();
                UIManager.Instance.OpenUI<CanvasWin>();
                break;

            case GameState.Fail:
                Time.timeScale = 0f;
                AudioManager.Instance?.PlayLose();
                UIManager.Instance.OpenUI<CanvasFail>();
                break;
        }
    }

    // ================== BUTTON / UI CALL ==================

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

    public void StartRealGame()
    {
        UIManager.Instance.CloseUIDirectly<PanelDemo>();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(1);
        }
    }

    void ShowCurrentLevel()
    {
        if (LevelManager.Instance == null) return;
        if (LevelManager.Instance.CurrentIndex <= 0) return;

        var canvasUI = UIManager.Instance.GetUI<CanvasGamePlay>();
        if (canvasUI != null)
        {
            canvasUI.ShowLevel(LevelManager.Instance.CurrentIndex);
        }
    }
}

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
    public EnemySpawner enemySpawner;          // script EnemySpawner
    [Tooltip("GameObject chứa EnemySpawner, sẽ bật / tắt khi mưa")]
    public GameObject enemySpawnerGO;          // object bật tắt
    [Tooltip("Parent để chứa tất cả enemy spawn ra (để dễ clear)")]
    public Transform enemyRoot;                // parent enemies

    [Header("Rain Settings")]
    [Tooltip("Thời gian chờ thêm sau khi mưa dừng rồi mới xét WIN / Replay demo")]
    public float surviveExtraTime = 3f;

    int  totalBlocks;      // tổng số ClickFall trong level
    int  fallenBlocks;     // số block đã click rơi
    bool ballHit      = false;  // ball đã từng trúng enemy chưa
    bool rainFinished = false;

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

        ClickFall[] blocks = levelRoot.GetComponentsInChildren<ClickFall>(true);
        totalBlocks  = blocks.Length;
        fallenBlocks = 0;

        Debug.Log($"[GameManager] Setup level – blocks: {totalBlocks}");

        bool isDemoLevel = (LevelManager.Instance != null && LevelManager.Instance.CurrentIndex == 0);

        if (isDemoLevel)
        {
            // ===== LEVEL DEMO =====
            SetState(GameState.Demo);
            UIManager.Instance.OpenUI<PanelDemo>();      // hiện PanelDemo
        }
        else
        {
            // ===== LEVEL THẬT =====
            SetState(GameState.Gameplay);
            ShowCurrentLevel();                          // hiện "LEVEL X" trên CanvasGamePlay
        }

        // cả demo lẫn gameplay: nếu không có block thì mưa luôn
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

    // Gọi từ ClickFall khi block bắt đầu rơi lần đầu tiên.
    public void NotifyBlockFallen(ClickFall block)
    {
        // Cho phép cả Demo và Gameplay dùng chung luật
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
            Debug.LogWarning("[GameManager] Chưa gán EnemySpawner hoặc enemySpawnerGO");
            return;
        }

        enemySpawnerGO.SetActive(true);
        enemySpawner.StartRain();

        Debug.Log("[GameManager] Start rain");
    }

    // GỌI từ EnemySpawner khi coroutine mưa kết thúc.
    public void OnRainFinished()
    {
        Debug.Log("[GameManager] Rain finished");
        rainFinished = true;

        if (CurrentState == GameState.Demo)
        {
            // DEMO: chờ 3s rồi tự Replay lại level demo
            StartCoroutine(DemoAutoReplay());
            return;
        }

        // GAMEPLAY THẬT: nếu không trúng enemy thì xét WIN
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
            RestartLevel();    // Replay lại chính level demo (index 0)
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

    // GỌI từ Ball (ClickFallSpine / BallSpine) khi va chạm enemy.
    public void OnBallHitEnemy()
    {
        // Trong Demo: bỏ qua, không Fail, chỉ cho xem hiệu ứng
        if (CurrentState == GameState.Demo)
            return;

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

    // =========== HÀM CHO PANEL DEMO GỌI ===========

    // Gọi từ PanelDemo.OnClickPlay()
    public void StartRealGame()
    {
        // Đóng panel demo
        UIManager.Instance.CloseUIDirectly<PanelDemo>();

        // Bắt đầu chơi level thật đầu tiên (index 1)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(1);
        }
    }

    void ShowCurrentLevel()
    {
        if (LevelManager.Instance == null) return;

        // Không show level text cho demo (index 0)
        if (LevelManager.Instance.CurrentIndex <= 0) return;

        var canvasUI = UIManager.Instance.GetUI<CanvasGamePlay>();
        if (canvasUI != null)
        {
            canvasUI.ShowLevel(LevelManager.Instance.CurrentIndex);
        }
    }
}

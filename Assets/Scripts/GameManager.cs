using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
 
[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS & PROPERTIES
    // ==========================================
    public static GameManager Instance { get; private set; }

    public int lives { get; private set; } = 3;
    public int coins { get; private set; } = 0;
    public int score { get; private set; } = 0;
    public int time  { get; set; } = 150;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private bool _levelComplete  = false;
    private bool _isFirstLaunch  = true;

    private GameObject _startPanel;
    private GameObject _victoryPanel;
    private GameObject _gameOverPanel;

    private TMP_InputField _nameInput;
    private string _playerName = "";

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const int DEFAULT_LIVES = 3;
    private const int DEFAULT_TIME = 150;
    private const int NAME_CHAR_LIMIT = 5;

    // ==========================================
    // 4. MONOBEHAVIOUR METHODS
    // ==========================================
    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayerPrefs.DeleteAll();
        Application.targetFrameRate = 60;
        FindPanels();

        if (_isFirstLaunch)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            StartGameLogic();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _levelComplete = false;
        FindPanels();

        if (_isFirstLaunch)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            StartGameLogic();
        }
    }

    // ==========================================
    // 5. PUBLIC METHODS
    // ==========================================
    public void AddScore(int amount)
    {
        score += amount;
    }

    public void AddCoin()
    {
        coins++;
    }

    public void AddLife()
    {
        lives++;
    }

    public void StartWinSequence()
    {
        _levelComplete = true;
        CancelInvoke(nameof(TickTime));
    }

    public void ShowVictoryUI()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayVictory();
        }

        LeaderboardManager lb = FindAnyObjectByType<LeaderboardManager>();
        if (lb != null)
        {
            lb.SaveScore(_playerName, score);
        }

        if (_victoryPanel != null)
        {
            _victoryPanel.SetActive(true);
        }
    }

    public void WinLevel()
    {
        StartWinSequence();
        ShowVictoryUI();
    }

    public void ResetLevel(float delay)
    {
        if (_levelComplete)
        {
            return;
        }

        CancelInvoke(nameof(TickTime));
        StartCoroutine(ResetAfterDelay(delay));
    }

    public void ResetLevel()
    {
        if (_levelComplete)
        {
            return;
        }

        lives--;

        if (lives > 0)
        {
            coins = 0;
            score = 0;
            time  = DEFAULT_TIME;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            ShowGameOver();
            coins = 0;
            score = 0;
            time  = DEFAULT_TIME;
        }
    }

    public void StartGame()
    {
        _playerName = ((_nameInput != null) && !string.IsNullOrWhiteSpace(_nameInput.text))
            ? _nameInput.text.Trim()
            : "Mario";

        ResetStats();
        _isFirstLaunch = false;

        if (_startPanel != null)
        {
            _startPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        StartGameLogic();
    }

    public void RestartGame()
    {
        ResetStats();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NewGame()
    {
        RestartGame();
    }

    public void GoToStart()
    {
        CancelInvoke(nameof(TickTime));
        ResetStats();
        _isFirstLaunch = true;
        Time.timeScale = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Application quitting...");
        Application.Quit();
    }

    public void TriggerPlayerJump()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.OnJumpButtonPressed();
        }
    }

    // ==========================================
    // 6. PRIVATE METHODS
    // ==========================================
    private void FindPanels()
    {
        _victoryPanel  = null;
        _gameOverPanel = null;
        _startPanel    = null;
        _nameInput     = null;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.scene.name == null || go.scene.name == "")
            {
                continue;
            }

            if (go.scene.name == "DontDestroyOnLoad")
            {
                continue;
            }

            switch (go.name)
            {
                case "VictoryPanel":
                    _victoryPanel = go;
                    break;
                case "GameOverPanel":
                    _gameOverPanel = go;
                    break;
                case "StartPanel":
                    _startPanel = go;
                    _nameInput  = go.GetComponentInChildren<TMP_InputField>(true);
                    if (_nameInput != null)
                    {
                        _nameInput.characterLimit = NAME_CHAR_LIMIT;
                    }
                    break;
            }
        }

        if (_victoryPanel != null)
        {
            BindButton(_victoryPanel,  "ExitButton", GoToStart);
            BindButton(_victoryPanel,  "Restart",    RestartGame);
        }

        if (_gameOverPanel != null)
        {
            BindButton(_gameOverPanel, "ExitButton", GoToStart);
            BindButton(_gameOverPanel, "Restart",    RestartGame);
        }

        if (_startPanel != null)
        {
            _startPanel.SetActive(_isFirstLaunch);
            BindButton(_startPanel, "StartClickButton", StartGame);
            BindButton(_startPanel, "ExitClickButton",  QuitGame);
        }

        if (_victoryPanel  == null) Debug.LogWarning("FindPanels: 'VictoryPanel' not found!");
        if (_gameOverPanel == null) Debug.LogWarning("FindPanels: 'GameOverPanel' not found!");
        if (_startPanel    == null) Debug.LogWarning("FindPanels: 'StartPanel' not found!");
        if (_nameInput     == null) Debug.LogWarning("FindPanels: TMP_InputField not found in StartPanel!");
    }

    private void BindButton(GameObject panel, string namePart, UnityEngine.Events.UnityAction action)
    {
        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.gameObject.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
            }
        }
    }

    private void StartGameLogic()
    {
        CancelInvoke(nameof(TickTime));
        InvokeRepeating(nameof(TickTime), 1f, 1f);
    }

    private void TickTime()
    {
        if (_levelComplete)
        {
            return;
        }

        if (time > 0)
        {
            time--;
        }
        else
        {
            ResetLevel();
        }
    }

    private void ShowGameOver()
    {
        CancelInvoke(nameof(TickTime));

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayDeath();
        }

        LeaderboardManager lb = FindAnyObjectByType<LeaderboardManager>();
        if (lb != null)
        {
            lb.SaveScore(_playerName, score);
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
    }

    private void ResetStats()
    {
        _levelComplete = false;
        lives = DEFAULT_LIVES;
        coins = 0;
        score = 0;
        time  = DEFAULT_TIME;
    }

    // ==========================================
    // 7. COROUTINES
    // ==========================================
    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetLevel();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Central game state manager. Handles lives, score, coins, time, UI panels,
/// and multiplayer mode. Persists across scene loads.
/// </summary>
[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC PROPERTIES
    // ==========================================
    public static GameManager Instance { get; private set; }

    public int lives       { get; private set; } = 3;
    public int coins       { get; private set; } = 0;
    public int score       { get; private set; } = 0;
    public int time        { get; set; }         = 150;
    public int PlayerCount { get; private set; } = 1;
    public int redCoins    { get; private set; } = 0;
    public int greenCoins  { get; private set; } = 0;

    // ==========================================
    // 2. PUBLIC FIELDS
    // ==========================================
    [Header("Co-op Spawning References")]
    public GameObject player2Prefab;
    public Transform  player2SpawnPoint;

    [Header("Co-op Objects")]
    public GameObject coopObjects;

    // ==========================================
    // 3. PRIVATE FIELDS
    // ==========================================
    private bool _levelComplete = false;
    private bool _isFirstLaunch = true;

    private GameObject     _startPanel;
    private GameObject     _victoryPanel;
    private GameObject     _gameOverPanel;
    private TMP_InputField _nameInput;
    private string         _playerName = "";

    // ==========================================
    // 4. CONSTANTS
    // ==========================================
    private const int DEFAULT_LIVES   = 3;
    private const int DEFAULT_TIME    = 150;
    private const int NAME_CHAR_LIMIT = 5;

    // ==========================================
    // 5. MONOBEHAVIOUR METHODS
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

    // ==========================================
    // 6. PUBLIC METHODS
    // ==========================================
    public void AddScore(int amount) => score += amount;
    public void AddCoin()            => coins++;
    public void AddLife()            => lives++;

    public void AddRedCoin()
    {
        redCoins++;
        coins++;
    }

    public void AddGreenCoin()
    {
        greenCoins++;
        coins++;
    }

    public void StartWinSequence()
    {
        _levelComplete = true;
        CancelInvoke(nameof(TickTime));
    }

    public void ShowVictoryUI()
    {
        Time.timeScale = 0f;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayVictory();
        }

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SaveScore(_playerName, score);
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
        ResetRoundStats();

        if (lives > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            ShowGameOver();
        }
    }

    public void StartOnePlayerGame()
    {
        StartGame(playerCount: 1);
    }

    public void StartTwoPlayerGame()
    {
        StartGame(playerCount: 2);
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
        Time.timeScale  = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
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
    // 7. PRIVATE METHODS
    // ==========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _levelComplete = false;
        FindPanels();

        if (_isFirstLaunch)
        {
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = 1f;
        StartGameLogic();
        RefreshCameraTargets();
        StartCoroutine(ActivateCoopObjectsDelayed(PlayerCount == 2));
    }

    private void StartGame(int playerCount)
    {
        _playerName = ResolvePlayerName();
        PlayerCount    = playerCount;
        _isFirstLaunch = false;

        PlayerPrefs.SetInt("PlayerCount", PlayerCount);
        PlayerPrefs.SetString("PlayerName", _playerName);
        PlayerPrefs.Save();

        ResetStats();

        if (_startPanel != null)
        {
            _startPanel.SetActive(false);
        }

        ActivateCoopObjects(playerCount == 2);

        Time.timeScale = 1f;
        StartGameLogic();
        RefreshCameraTargets();
    }

    private void ActivateCoopObjects(bool isActive)
    {
        if (coopObjects == null)
        {
            CoopObjectsRegistrar[] registrars = FindObjectsByType<CoopObjectsRegistrar>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            if (registrars.Length > 0)
            {
                coopObjects = registrars[0].gameObject;
            }
        }

        if (coopObjects != null)
        {
            coopObjects.SetActive(isActive);
        }
    }

    private void RefreshCameraTargets()
    {
        CameraScrolling camera = Camera.main != null
            ? Camera.main.GetComponent<CameraScrolling>()
            : null;

        if (camera == null)
        {
            return;
        }

        PlayerMovement player1 = FindPlayerByIndex(1);

        if (player1 != null)
        {
            camera.player1 = player1.transform;
        }

        if (PlayerCount == 2)
        {
            GameObject player2Object = SpawnPlayer2();

            if (player2Object != null)
            {
                camera.player2 = player2Object.transform;
            }
        }
        else
        {
            camera.player2 = null;
        }
    }

    private GameObject SpawnPlayer2()
    {
        if (player2Prefab == null)
        {
            Debug.LogWarning("GameManager: player2Prefab is not assigned in Inspector!");
            return null;
        }

        PlayerMovement existingPlayer2 = FindPlayerByIndex(2);

        if (existingPlayer2 != null)
        {
            return existingPlayer2.gameObject;
        }

        Vector3 spawnPosition = ResolvePlayer2SpawnPosition();

        GameObject player2Object = Instantiate(player2Prefab, spawnPosition, Quaternion.identity);
        player2Object.name = "Mario 2";

        PlayerMovement movement = player2Object.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.playerIndex = 2;
        }

        return player2Object;
    }

    private Vector3 ResolvePlayer2SpawnPosition()
    {
        if (player2SpawnPoint != null)
        {
            return player2SpawnPoint.position;
        }

        PlayerMovement player1 = FindPlayerByIndex(1);

        if (player1 != null)
        {
            return player1.transform.position + Vector3.right * 1.5f;
        }

        return Vector3.zero;
    }

    private PlayerMovement FindPlayerByIndex(int index)
    {
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (PlayerMovement player in allPlayers)
        {
            if (player != null && player.playerIndex == index)
            {
                return player;
            }
        }

        return null;
    }

    private void FindPanels()
    {
        FindUIObjects();
        BindAllButtons();
        ValidatePanels();
    }

    private void FindUIObjects()
    {
        _victoryPanel  = null;
        _gameOverPanel = null;
        _startPanel    = null;
        _nameInput     = null;

        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (string.IsNullOrEmpty(go.scene.name))
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

        if (_startPanel != null)
        {
            _startPanel.SetActive(_isFirstLaunch);
        }
    }

    private void BindAllButtons()
    {
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
            BindButton(_startPanel, "OnePlayerButton",  StartOnePlayerGame);
            BindButton(_startPanel, "TwoPlayersButton", StartTwoPlayerGame);
            BindButton(_startPanel, "ExitClickButton",  QuitGame);
        }
    }

    private void ValidatePanels()
    {
        if (_victoryPanel  == null) Debug.LogWarning("GameManager: 'VictoryPanel' not found!");
        if (_gameOverPanel == null) Debug.LogWarning("GameManager: 'GameOverPanel' not found!");
        if (_startPanel    == null) Debug.LogWarning("GameManager: 'StartPanel' not found!");
        if (_nameInput     == null) Debug.LogWarning("GameManager: TMP_InputField not found in StartPanel!");
    }

    private void BindButton(GameObject panel, string namePart, UnityEngine.Events.UnityAction action)
    {
        foreach (Button btn in panel.GetComponentsInChildren<Button>(true))
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
        Time.timeScale = 0f;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayDeath();
        }

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SaveScore(_playerName, score);
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
    }

    private void ResetRoundStats()
    {
        coins      = 0;
        score      = 0;
        time       = DEFAULT_TIME;
        redCoins   = 0;
        greenCoins = 0;
    }

    private void ResetStats()
    {
        _levelComplete = false;
        lives          = DEFAULT_LIVES;
        ResetRoundStats();
    }

    private string ResolvePlayerName()
    {
        bool hasValidInput = _nameInput != null && !string.IsNullOrWhiteSpace(_nameInput.text);
        return hasValidInput ? _nameInput.text.Trim() : "Player";
    }

    // ==========================================
    // 8. COROUTINES
    // ==========================================
    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetLevel();
    }

    private IEnumerator ActivateCoopObjectsDelayed(bool isActive)
    {
        yield return null;
        ActivateCoopObjects(isActive);
    }
}
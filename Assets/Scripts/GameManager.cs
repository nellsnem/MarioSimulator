using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC FIELDS
    // ==========================================
    public static GameManager Instance { get; private set; }

    public int Lives       { get; private set; } = DefaultLives;
    public int Coins       { get; private set; } = 0;
    public int Score       { get; private set; } = 0;
    public int GameTime    { get; set; }         = DefaultTime;
    public int PlayerCount { get; private set; } = 1;
    public int RedCoins    { get; private set; } = 0;
    public int GreenCoins  { get; private set; } = 0;

    [Header("Co-op Spawning References")]
    public GameObject Player2Prefab;
    public Transform  Player2SpawnPoint;

    [Header("Co-op Objects")]
    public GameObject CoopObjects;

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private bool _isLevelComplete = false;
    private bool _isFirstLaunch   = true;

    private GameObject     _startPanel;
    private GameObject     _victoryPanel;
    private GameObject     _gameOverPanel;
    private TMP_InputField _nameInput;
    private string         _playerName = "";

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const int DefaultLives    = 3;
    private const int DefaultTime     = 150;
    private const int NameCharLimit   = 5;

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
        ApplyFirstLaunchTimeScale();
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
    // 5. PUBLIC METHODS
    // ==========================================
    public void AddScore(int amount) => Score += amount;
    public void AddCoin()            => Coins++;
    public void AddLife()            => Lives++;

    public void AddRedCoin()
    {
        RedCoins++;
        Coins++;
    }

    public void AddGreenCoin()
    {
        GreenCoins++;
        Coins++;
    }

    public void StartWinSequence()
    {
        _isLevelComplete = true;
        CancelInvoke(nameof(TickTime));
    }

    public void ShowVictoryUI()
    {
        UnityEngine.Time.timeScale = 0f;
        MusicManager.Instance?.PlayVictory();
        LeaderboardManager.Instance?.SaveScore(_playerName, Score);

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
        if (_isLevelComplete)
        {
            return;
        }

        CancelInvoke(nameof(TickTime));
        StartCoroutine(ResetAfterDelay(delay));
    }

    public void ResetLevel()
    {
        if (_isLevelComplete)
        {
            return;
        }

        Lives--;
        ResetRoundStats();

        if (Lives > 0)
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
        UnityEngine.Time.timeScale = 1f;
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
        _isFirstLaunch             = true;
        UnityEngine.Time.timeScale = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ==========================================
    // 6. PRIVATE METHODS
    // ==========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isLevelComplete = false;
        FindPanels();

        if (_isFirstLaunch)
        {
            UnityEngine.Time.timeScale = 0f;
            return;
        }

        UnityEngine.Time.timeScale = 1f;
        StartGameLogic();
        RefreshCameraTargets();
        StartCoroutine(ActivateCoopObjectsDelayed(PlayerCount == 2));
    }

    private void ApplyFirstLaunchTimeScale()
    {
        UnityEngine.Time.timeScale = _isFirstLaunch ? 0f : 1f;

        if (!_isFirstLaunch)
        {
            StartGameLogic();
        }
    }

    private void StartGame(int playerCount)
    {
        _playerName    = ResolvePlayerName();
        PlayerCount    = playerCount;
        _isFirstLaunch = false;

        SavePlayerPrefs();
        ResetStats();
        HideStartPanel();
        ActivateCoopObjects(playerCount == 2);

        UnityEngine.Time.timeScale = 1f;
        StartGameLogic();
        RefreshCameraTargets();
    }

    private void SavePlayerPrefs()
    {
        PlayerPrefs.SetInt("PlayerCount", PlayerCount);
        PlayerPrefs.SetString("PlayerName", _playerName);
        PlayerPrefs.Save();
    }

    private void HideStartPanel()
    {
        if (_startPanel != null)
        {
            _startPanel.SetActive(false);
        }
    }

    private void ActivateCoopObjects(bool isActive)
    {
        ResolveCoopObjects();

        if (CoopObjects != null)
        {
            CoopObjects.SetActive(isActive);
        }
    }

    private void ResolveCoopObjects()
    {
        if (CoopObjects != null)
        {
            return;
        }

        CoopObjectsRegistrar[] registrars = FindObjectsByType<CoopObjectsRegistrar>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (registrars.Length > 0)
        {
            CoopObjects = registrars[0].gameObject;
        }
    }

    private void RefreshCameraTargets()
    {
        CameraScrolling camera = ResolveCameraScrolling();

        if (camera == null)
        {
            return;
        }

        AssignPlayer1Camera(camera);

        if (PlayerCount == 2)
        {
            AssignPlayer2Camera(camera);
        }
        else
        {
            camera.player2 = null;
        }
    }

    private CameraScrolling ResolveCameraScrolling()
    {
        return Camera.main != null ? Camera.main.GetComponent<CameraScrolling>() : null;
    }

    private void AssignPlayer1Camera(CameraScrolling camera)
    {
        PlayerMovement player1 = FindPlayerByIndex(1);

        if (player1 != null)
        {
            camera.player1 = player1.transform;
        }
    }

    private void AssignPlayer2Camera(CameraScrolling camera)
    {
        GameObject player2Object = SpawnPlayer2();

        if (player2Object != null)
        {
            camera.player2 = player2Object.transform;
        }
    }

    private GameObject SpawnPlayer2()
    {
        if (Player2Prefab == null)
        {
            Debug.LogWarning("GameManager: Player2Prefab is not assigned in Inspector!");
            return null;
        }

        PlayerMovement existingPlayer2 = FindPlayerByIndex(2);

        if (existingPlayer2 != null)
        {
            return existingPlayer2.gameObject;
        }

        return InstantiatePlayer2();
    }

    private GameObject InstantiatePlayer2()
    {
        Vector3    spawnPosition = ResolvePlayer2SpawnPosition();
        GameObject player2Object = Instantiate(Player2Prefab, spawnPosition, Quaternion.identity);
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
        if (Player2SpawnPoint != null)
        {
            return Player2SpawnPoint.position;
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
            if (!IsSceneObject(go))
            {
                continue;
            }

            CacheUIPanel(go);
        }

        if (_startPanel != null)
        {
            _startPanel.SetActive(_isFirstLaunch);
        }
    }

    private bool IsSceneObject(GameObject go)
    {
        return !string.IsNullOrEmpty(go.scene.name) && go.scene.name != "DontDestroyOnLoad";
    }

    private void CacheUIPanel(GameObject go)
    {
        switch (go.name)
        {
            case "VictoryPanel":
                _victoryPanel = go;
                break;
            case "GameOverPanel":
                _gameOverPanel = go;
                break;
            case "StartPanel":
                CacheStartPanel(go);
                break;
        }
    }

    private void CacheStartPanel(GameObject go)
    {
        _startPanel = go;
        _nameInput  = go.GetComponentInChildren<TMP_InputField>(true);

        if (_nameInput != null)
        {
            _nameInput.characterLimit = NameCharLimit;
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
        if (_victoryPanel == null)
        {
            Debug.LogWarning("GameManager: 'VictoryPanel' not found!");
        }
        if (_gameOverPanel == null)
        {
            Debug.LogWarning("GameManager: 'GameOverPanel' not found!");
        } 
        if (_startPanel == null)
        {
            Debug.LogWarning("GameManager: 'StartPanel' not found!");
        } 
        if (_nameInput == null)
        {
            Debug.LogWarning("GameManager: TMP_InputField not found in StartPanel!");
        } 
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
        if (_isLevelComplete)
        {
            return;
        }

        if (GameTime  > 0)
        {
            GameTime --;
        }
        else
        {
            ResetLevel();
        }
    }

    private void ShowGameOver()
    {
        CancelInvoke(nameof(TickTime));
        UnityEngine.Time.timeScale = 0f;

        MusicManager.Instance?.PlayDeath();
        LeaderboardManager.Instance?.SaveScore(_playerName, Score);

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
    }

    private void ResetRoundStats()
    {
        Coins      = 0;
        Score      = 0;
        GameTime    = DefaultTime;
        RedCoins   = 0;
        GreenCoins = 0;
    }

    private void ResetStats()
    {
        _isLevelComplete = false;
        Lives            = DefaultLives;
        ResetRoundStats();
    }

    private string ResolvePlayerName()
    {
        bool hasValidInput = _nameInput != null && !string.IsNullOrWhiteSpace(_nameInput.text);
        return hasValidInput ? _nameInput.text.Trim() : "Player";
    }

    // ==========================================
    // 7. COROUTINES
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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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
    public int redCoins   { get; private set; } = 0;
    public int greenCoins { get; private set; } = 0;

    // ==========================================
    // 2. PUBLIC FIELDS
    // ==========================================
    [Header("Co-op Spawning References")]
    public GameObject player2Prefab;

    public Transform player2SpawnPoint;

    [Header("Co-op Objects")]
    public GameObject coopObjects;

    // ==========================================
    // 3. PRIVATE FIELDS
    // ==========================================
    private bool _levelComplete = false;
    private bool _isFirstLaunch = true;

    private GameObject _startPanel;
    private GameObject _victoryPanel;
    private GameObject _gameOverPanel;

    private TMP_InputField _nameInput;
    private string _playerName = "";

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

private IEnumerator ActivateCoopObjectsDelayed(bool isActive)
{
    yield return null; // чекаємо один кадр поки всі Awake спрацюють
    ActivateCoopObjects(isActive);
}

    // ==========================================
    // 6. PUBLIC METHODS
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

        if (lives > 0)
        {
            coins = 0;
            score = 0;
            time  = DEFAULT_TIME;
            redCoins   = 0; // ← додай
            greenCoins = 0; // ← додай
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            ShowGameOver();
            coins = 0;
            score = 0;
            time  = DEFAULT_TIME;
            redCoins   = 0; // ← додай
            greenCoins = 0; // ← додай
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
    // 7. PRIVATE METHODS
    // ==========================================
    private void StartGame(int playerCount)
    {
        _playerName = ((_nameInput != null) && !string.IsNullOrWhiteSpace(_nameInput.text))
            ? _nameInput.text.Trim()
            : "Player";

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
        CoopObjectsRegistrar registrar = 
            FindObjectsByType<CoopObjectsRegistrar>(
                FindObjectsInactive.Include, 
                FindObjectsSortMode.None
            ).Length > 0 
            ? FindObjectsByType<CoopObjectsRegistrar>(
                FindObjectsInactive.Include, 
                FindObjectsSortMode.None)[0] 
            : null;

        if (registrar != null)
        {
            coopObjects = registrar.gameObject;
        }
    }

    if (coopObjects != null)
    {
        coopObjects.SetActive(isActive);
    }
}

    private void RefreshCameraTargets()
    {
        CameraScrolling standardCam = Camera.main != null
            ? Camera.main.GetComponent<CameraScrolling>()
            : null;

        if (standardCam == null)
        {
            return;
        }

        PlayerMovement p1 = FindPlayerByIndex(1);
        if (p1 != null)
        {
            standardCam.player1 = p1.transform;
        }

        if (PlayerCount == 2)
        {
            GameObject p2Object = SpawnPlayer2();
            if (p2Object != null)
            {
                standardCam.player2 = p2Object.transform;
            }
        }
        else
        {
            standardCam.player2 = null;
        }
    }

    private GameObject SpawnPlayer2()
    {
        if (player2Prefab == null)
        {
            Debug.LogWarning("SpawnPlayer2: player2Prefab reference is missing in Inspector!");
            return null;
        }

        PlayerMovement existingP2 = FindPlayerByIndex(2);
        if (existingP2 != null)
        {
            return existingP2.gameObject;
        }

        Vector3 spawnPos = Vector3.zero;
        if (player2SpawnPoint != null)
        {
            spawnPos = player2SpawnPoint.position;
        }
        else
        {
            PlayerMovement p1 = FindPlayerByIndex(1);
            if (p1 != null)
            {
                spawnPos = p1.transform.position + Vector3.right * 1.5f;
            }
        }

        GameObject p2 = Instantiate(player2Prefab, spawnPos, Quaternion.identity);
        p2.name = "Mario 2";

        PlayerMovement pm = p2.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.playerIndex = 2;
        }

        return p2;
    }

    private PlayerMovement FindPlayerByIndex(int index)
    {
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include);
        foreach (PlayerMovement p in allPlayers)
        {
            if (p != null && p.playerIndex == index)
            {
                return p;
            }
        }
        return null;
    }

    private void FindPanels()
    {
        _victoryPanel  = null;
        _gameOverPanel = null;
        _startPanel    = null;
        _nameInput     = null;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
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
            BindButton(_startPanel, "OnePlayerButton",  StartOnePlayerGame);
            BindButton(_startPanel, "TwoPlayersButton", StartTwoPlayerGame);
            BindButton(_startPanel, "ExitClickButton",  QuitGame);
        }
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

    private void ResetStats()
    {
        _levelComplete = false;
        lives = DEFAULT_LIVES;
        coins = 0;
        score = 0;
        redCoins   = 0; // ← додай
        greenCoins = 0; 
        time  = DEFAULT_TIME;
    }

    // ==========================================
    // 8. COROUTINES
    // ==========================================
    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetLevel();
    }
}
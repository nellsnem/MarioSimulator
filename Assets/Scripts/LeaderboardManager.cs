using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages top-3 leaderboard persistence and UI display across scenes.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    // ==========================================
    // 1. PUBLIC PROPERTIES
    // ==========================================
    public static LeaderboardManager Instance { get; private set; }

    // ==========================================
    // 2. PRIVATE FIELDS
    // ==========================================
    private TextMeshProUGUI[] _leaderTexts;

    // Static readonly field — belongs in private fields, not constants
    private static readonly string[] _leaderTextNames = { "top1", "top2", "top3" };

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const int TOP_COUNT = 3;
    private const string LEADERBOARD_KEY = "LeaderboardData";

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

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        FindLeaderTexts();
        UpdateLeaderboardDisplay();
    }

    // ==========================================
    // 5. PUBLIC METHODS
    // ==========================================
    public void SaveScore(string playerName, int playerScore)
    {
        List<LeaderboardEntry> scores = LoadLeaderboard();
        scores.Add(new LeaderboardEntry(playerName, playerScore));

        List<LeaderboardEntry> topScores = scores
            .OrderByDescending(s => s.score)
            .Take(TOP_COUNT)
            .ToList();

        string json = JsonUtility.ToJson(new SerializationWrapper<LeaderboardEntry> { items = topScores });
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();

        UpdateLeaderboardDisplay();
    }

    public void UpdateLeaderboardDisplay()
    {
        if (_leaderTexts == null || _leaderTexts.Length == 0)
        {
            return;
        }

        List<LeaderboardEntry> scores = LoadLeaderboard();

        for (int i = 0; i < _leaderTexts.Length; i++)
        {
            if (_leaderTexts[i] == null)
            {
                continue;
            }

            _leaderTexts[i].text = i < scores.Count
                ? $"{i + 1}. {scores[i].name} — {scores[i].score}"
                : $"{i + 1}. ---";
        }
    }

    // ==========================================
    // 6. PRIVATE METHODS
    // ==========================================
    private void SubscribeEvents()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void UnsubscribeEvents()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindLeaderTexts();
        UpdateLeaderboardDisplay();
    }

    private void FindLeaderTexts()
    {
        _leaderTexts = new TextMeshProUGUI[_leaderTextNames.Length];
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < _leaderTextNames.Length; i++)
        {
            _leaderTexts[i] = FindLeaderText(all, _leaderTextNames[i]);
        }
    }

    private TextMeshProUGUI FindLeaderText(GameObject[] all, string targetName)
    {
        foreach (GameObject go in all)
        {
            if (go.name == targetName)
            {
                return go.GetComponent<TextMeshProUGUI>();
            }
        }

        return null;
    }

    private List<LeaderboardEntry> LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");

        if (string.IsNullOrEmpty(json))
        {
            return new List<LeaderboardEntry>();
        }

        SerializationWrapper<LeaderboardEntry> wrapper =
            JsonUtility.FromJson<SerializationWrapper<LeaderboardEntry>>(json);

        return wrapper?.items ?? new List<LeaderboardEntry>();
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public string name;
    public int    score;

    public LeaderboardEntry(string entryName, int entryScore)
    {
        name  = entryName;
        score = entryScore;
    }
}

[System.Serializable]
public class SerializationWrapper<T>
{
    public List<T> items;
}
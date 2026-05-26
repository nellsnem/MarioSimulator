using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;


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

    // ==========================================
    // 3. CONSTANTS
    // ==========================================
    private const int TOP_COUNT = 3;

    private static readonly string[] _leaderTextNames = { "top1", "top2", "top3" };

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

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        FindLeaderTexts();
        UpdateLeaderboardDisplay();
    }

    // ==========================================
    // 5. PUBLIC METHODS
    // ==========================================
    public void SaveScore(string name, int score)
    {
        List<LeaderboardEntry> scores = LoadLeaderboard();
        scores.Add(new LeaderboardEntry(name, score));

        // Sort descending by score and keep top entries only
        var topScores = scores.OrderByDescending(s => s.score).Take(TOP_COUNT).ToList();

        string json = JsonUtility.ToJson(new SerializationWrapper<LeaderboardEntry> { items = topScores });
        PlayerPrefs.SetString("LeaderboardData", json);
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
            foreach (GameObject go in all)
            {
                if (go.name == _leaderTextNames[i])
                {
                    _leaderTexts[i] = go.GetComponent<TextMeshProUGUI>();
                    break;
                }
            }
        }
    }

    private List<LeaderboardEntry> LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString("LeaderboardData", "");

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

    public LeaderboardEntry(string name, int score)
    {
        this.name  = name;
        this.score = score;
    }
}

[System.Serializable]
public class SerializationWrapper<T>
{
    public List<T> items;
}
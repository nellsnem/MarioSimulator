using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{ 
    public static LeaderboardManager Instance { get; private set; }
 
    private TextMeshProUGUI[] leaderTexts;

    private const int TopCount = 3;
 
    private static readonly string[] LeaderTextNames = { "top1", "top2", "top3" };
 
    private void Awake()
    {
        if (Instance != null) { DestroyImmediate(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindLeaderTexts();
        UpdateLeaderboardDisplay();
    }

    private void Start()
    {
        FindLeaderTexts();
        UpdateLeaderboardDisplay();
    }

     private void FindLeaderTexts()
    {
        leaderTexts = new TextMeshProUGUI[LeaderTextNames.Length];
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < LeaderTextNames.Length; i++)
        {
            foreach (GameObject go in all)
            {
                if (string.IsNullOrEmpty(go.scene.name)) continue;
                if (go.scene.name == "DontDestroyOnLoad") continue;
                if (go.name == LeaderTextNames[i])
                {
                    leaderTexts[i] = go.GetComponent<TextMeshProUGUI>();
                    break;
                }
            }
            if (leaderTexts[i] == null)
                Debug.LogWarning($"LeaderboardManager: '{LeaderTextNames[i]}' не знайдено в сцені!");
        }
    }
 
    public void SaveScore(string playerName, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Mario";

        List<LeaderboardEntry> scores = LoadLeaderboard();
        scores.Add(new LeaderboardEntry(playerName, score));
 
        List<LeaderboardEntry> top = scores
            .OrderByDescending(e => e.score)
            .Take(TopCount)
            .ToList();

        string json = JsonUtility.ToJson(new SerializationWrapper<LeaderboardEntry>(top));
        PlayerPrefs.SetString("LeaderboardData", json);
        PlayerPrefs.Save();

        UpdateLeaderboardDisplay();
    }

     public void UpdateLeaderboardDisplay()
    {
        if (leaderTexts == null || leaderTexts.Length == 0)
        {
             return;
        }

        List<LeaderboardEntry> scores = LoadLeaderboard();

        for (int i = 0; i < leaderTexts.Length; i++)
        {
            if (leaderTexts[i] == null) continue;

            leaderTexts[i].text = i < scores.Count
                ? $"{i + 1}. {scores[i].name} — {scores[i].score}"
                : $"{i + 1}. ---";
        }
    }
 
    private List<LeaderboardEntry> LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString("LeaderboardData", "");

        if (string.IsNullOrEmpty(json))
            return new List<LeaderboardEntry>();

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
    public SerializationWrapper(List<T> items) => this.items = items;
}
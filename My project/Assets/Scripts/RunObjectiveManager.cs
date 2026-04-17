using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunObjectiveManager : MonoBehaviour
{
    private static readonly string[] GameplaySceneNames = { "Level", "map" };

    public static RunObjectiveManager Instance { get; private set; }

    public int CompletedObjectiveCount { get; private set; }
    public int TotalObjectiveCount => 3;
    public bool IsRunFinished { get; private set; }
    public string LastRunSummary { get; private set; } = "No run data.";

    [Header("Objective Targets")]
    [SerializeField] private float surviveSecondsTarget = 180f;
    [SerializeField] private int killsTarget = 25;
    [SerializeField] private int streakTarget = 5;

    private bool surviveCompleted;
    private bool killsCompleted;
    private bool streakCompleted;

    private Text objectiveText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!IsGameplayScene(SceneManager.GetActiveScene().name))
        {
            return;
        }

        if (FindAnyObjectByType<RunObjectiveManager>() != null)
        {
            return;
        }

        GameObject runtimeRoot = new GameObject("RunObjectiveManager");
        runtimeRoot.AddComponent<RunObjectiveManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetRunState();
    }

    private void OnEnable()
    {
        HUDManager.KillRegistered += HandleKillRegistered;
        PlayerHealth.PlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        HUDManager.KillRegistered -= HandleKillRegistered;
        PlayerHealth.PlayerDied -= HandlePlayerDied;
    }

    private void Update()
    {
        if (!IsGameplayScene(SceneManager.GetActiveScene().name))
        {
            return;
        }

        if (!IsRunFinished)
        {
            EvaluateSurvivalObjective();
            UpdateObjectiveText();
        }
    }

    public string GetLiveObjectiveText()
    {
        float elapsed = Timer.Instance != null ? Timer.Instance.ElapsedTime : 0f;
        int kills = HUDManager.Instance != null ? HUDManager.Instance.TotalKills : 0;
        int bestStreak = HUDManager.Instance != null ? HUDManager.Instance.BestKillStreak : 0;

        string surviveLine = (surviveCompleted ? "[DONE]" : "[TODO]") + " Survive " + FormatTime(surviveSecondsTarget) + " (" + FormatTime(elapsed) + ")";
        string killsLine = (killsCompleted ? "[DONE]" : "[TODO]") + " Kill " + killsTarget + " enemies (" + kills + "/" + killsTarget + ")";
        string streakLine = (streakCompleted ? "[DONE]" : "[TODO]") + " Reach streak " + streakTarget + " (best " + bestStreak + ")";

        return "RUN OBJECTIVES\n" + surviveLine + "\n" + killsLine + "\n" + streakLine;
    }

    public string BuildPostRunSummary()
    {
        HUDManager hud = HUDManager.Instance;
        float elapsed = Timer.Instance != null ? Timer.Instance.ElapsedTime : 0f;

        int score = hud != null ? hud.Score : 0;
        int kills = hud != null ? hud.TotalKills : 0;
        int bestStreak = hud != null ? hud.BestKillStreak : 0;

        return "RUN SUMMARY\n"
            + "Time Survived: " + FormatTime(elapsed) + "\n"
            + "Score: " + score + "\n"
            + "Kills: " + kills + "\n"
            + "Best Streak: " + bestStreak + "\n"
            + "Objectives: " + CompletedObjectiveCount + "/" + TotalObjectiveCount;
    }

    private void ResetRunState()
    {
        CompletedObjectiveCount = 0;
        IsRunFinished = false;
        surviveCompleted = false;
        killsCompleted = false;
        streakCompleted = false;
        LastRunSummary = "No run data.";
    }

    private void HandleKillRegistered(int totalKills, int currentStreak)
    {
        if (IsRunFinished)
        {
            return;
        }

        if (!killsCompleted && totalKills >= killsTarget)
        {
            killsCompleted = true;
            CompletedObjectiveCount++;
        }

        if (!streakCompleted && currentStreak >= streakTarget)
        {
            streakCompleted = true;
            CompletedObjectiveCount++;
        }
    }

    private void EvaluateSurvivalObjective()
    {
        if (surviveCompleted)
        {
            return;
        }

        float elapsed = Timer.Instance != null ? Timer.Instance.ElapsedTime : 0f;
        if (elapsed >= surviveSecondsTarget)
        {
            surviveCompleted = true;
            CompletedObjectiveCount++;
        }
    }

    private void HandlePlayerDied()
    {
        if (IsRunFinished)
        {
            return;
        }

        IsRunFinished = true;
        LastRunSummary = BuildPostRunSummary();
    }

    private void UpdateObjectiveText()
    {
        EnsureObjectiveText();
        if (objectiveText == null)
        {
            return;
        }

        objectiveText.text = GetLiveObjectiveText();
    }

    private void EnsureObjectiveText()
    {
        if (objectiveText != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        Transform existing = canvas.transform.Find("ObjectiveText");
        if (existing == null)
        {
            GameObject objectiveObject = new GameObject("ObjectiveText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            objectiveObject.transform.SetParent(canvas.transform, false);

            RectTransform rect = objectiveObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -56f);
            rect.sizeDelta = new Vector2(460f, 120f);

            objectiveText = objectiveObject.GetComponent<Text>();
            objectiveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            objectiveText.alignment = TextAnchor.UpperLeft;
            objectiveText.fontSize = 18;
            objectiveText.color = new Color(0.95f, 0.98f, 0.9f, 0.95f);
        }
        else
        {
            objectiveText = existing.GetComponent<Text>();
        }
    }

    private static string FormatTime(float seconds)
    {
        int safeSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int minutes = safeSeconds / 60;
        int remain = safeSeconds % 60;
        return string.Format("{0:00}:{1:00}", minutes, remain);
    }

    private static bool IsGameplayScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        for (int i = 0; i < GameplaySceneNames.Length; i++)
        {
            if (string.Equals(sceneName, GameplaySceneNames[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

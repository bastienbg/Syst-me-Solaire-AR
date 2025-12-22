using System;
using System.Collections.Generic;
using UnityEngine;

public class ARGameManager : MonoBehaviour
{
    public static ARGameManager Instance { get; private set; }

    public enum ObjectiveType
    {
        SpawnAny,
        SpawnSpecific,
        ViewInfoAny,
        ViewInfoSpecific,
        DestroyAny,
        DestroySpecific
    }

    [Serializable]
    public class Objective
    {
        public ObjectiveType type;
        public string planetId;           // utilisé si Specific
        public int targetCount = 1;

        [Header("Points")]
        public int pointsPerProgress = 10; // points à chaque fois que tu avances l'objectif
        public int bonusOnComplete = 50;   // bonus quand l'objectif est complété
    }

    [Serializable]
    public class LevelConfig
    {
        public string name = "Level";
        public float durationSeconds = 60f;
        public List<Objective> objectives = new List<Objective>();
        public int bonusOnLevelComplete = 100;
    }

    [Header("Config")]
    public List<LevelConfig> levels = new List<LevelConfig>();

    [Header("HUD")]
    public GameHUD hud;

    [Header("Etat")]
    public int score = 0;

    private int levelIndex = 0;
    private float timeLeft = 0f;
    private bool levelRunning = false;

    private int[] objectiveProgress;
    private bool[] objectiveCompleted;

    private int totalSpawned = 0;
    private int totalInfoViewed = 0;
    private int totalDestroyed = 0;

    private HashSet<string> spawnedPlanets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> infoPlanets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> destroyedPlanets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Optionnel: tu peux démarrer direct au lancement, sinon via LevelStarterOnSun (recommandé)
        SetupLevel(0);
        UpdateHUD();
    }

    private void Update()
    {
        if (!levelRunning) return;

        timeLeft -= Time.deltaTime;
        if (hud != null) hud.SetTimer(timeLeft);

        if (timeLeft <= 0f)
        {
            FailLevel();
        }
    }

    public void StartLevel()
    {
        if (levels == null || levels.Count == 0) return;
        levelRunning = true;
        timeLeft = levels[levelIndex].durationSeconds;
        if (hud != null) hud.ShowMessage("GO !");
        UpdateHUD();
    }

    public void StopLevel()
    {
        levelRunning = false;
        if (hud != null) hud.ShowMessage("Pause");
    }

    private void SetupLevel(int index)
    {
        levelIndex = Mathf.Clamp(index, 0, Mathf.Max(0, levels.Count - 1));
        var lvl = levels[levelIndex];

        objectiveProgress = new int[lvl.objectives.Count];
        objectiveCompleted = new bool[lvl.objectives.Count];

        timeLeft = lvl.durationSeconds;
        levelRunning = false;

        SyncObjectivesFromGlobalStats();
        UpdateHUD();

    }

    private void UpdateHUD()
    {
        if (hud == null || levels.Count == 0) return;

        hud.SetLevel(levelIndex);
        hud.SetScore(score);
        hud.SetTimer(timeLeft);

        var lvl = levels[levelIndex];
        string txt = "";
        for (int i = 0; i < lvl.objectives.Count; i++)
        {
            var o = lvl.objectives[i];
            int cur = objectiveProgress[i];
            int tgt = o.targetCount;
            string label = ObjectiveLabel(o);
            string done = objectiveCompleted[i] ? " [OK]" : "";
            txt += $"{label} : {cur}/{tgt}{done}\n";
        }
        hud.SetObjectives(txt.TrimEnd());
    }

    private string ObjectiveLabel(Objective o)
    {
        return o.type switch
        {
            ObjectiveType.SpawnAny => "Faire apparaître des planètes",
            ObjectiveType.SpawnSpecific => $"Faire apparaître {o.planetId}",
            ObjectiveType.ViewInfoAny => "Lire des infos (Info target)",
            ObjectiveType.ViewInfoSpecific => $"Lire infos de {o.planetId}",
            ObjectiveType.DestroyAny => "Détruire des planètes (astéroïde)",
            ObjectiveType.DestroySpecific => $"Détruire {o.planetId}",
            _ => "Objectif"
        };
    }


    public void RegisterSpawn(string planetId)
    {
        if (!string.IsNullOrEmpty(planetId) && spawnedPlanets.Add(planetId))
            totalSpawned++;

        RegisterAction(ObjectiveType.SpawnAny, ObjectiveType.SpawnSpecific, planetId);
    }

    public void RegisterInfo(string planetId)
    {
        if (!string.IsNullOrEmpty(planetId) && infoPlanets.Add(planetId))
            totalInfoViewed++;

        RegisterAction(ObjectiveType.ViewInfoAny, ObjectiveType.ViewInfoSpecific, planetId);
    }

    public void RegisterDestroy(string planetId)
    {
        if (!string.IsNullOrEmpty(planetId) && destroyedPlanets.Add(planetId))
            totalDestroyed++;

        RegisterAction(ObjectiveType.DestroyAny, ObjectiveType.DestroySpecific, planetId);
    }


    private void RegisterAction(ObjectiveType anyType, ObjectiveType specificType, string planetId)
    {
        if (!levelRunning || levels.Count == 0) return;

        var lvl = levels[levelIndex];

        for (int i = 0; i < lvl.objectives.Count; i++)
        {
            if (objectiveCompleted[i]) continue;

            var o = lvl.objectives[i];

            bool matches =
                (o.type == anyType) ||
                (o.type == specificType && !string.IsNullOrEmpty(o.planetId) && string.Equals(o.planetId, planetId, StringComparison.OrdinalIgnoreCase));

            if (!matches) continue;

            // Progress
            objectiveProgress[i] = Mathf.Min(o.targetCount, objectiveProgress[i] + 1);
            score += Mathf.Max(0, o.pointsPerProgress);

            // Completed ?
            if (objectiveProgress[i] >= o.targetCount)
            {
                objectiveCompleted[i] = true;
                score += Mathf.Max(0, o.bonusOnComplete);
                if (hud != null) hud.ShowMessage($"Objectif terminé : {ObjectiveLabel(o)}");
            }

            UpdateHUD();
            CheckLevelComplete();
            return;
        }
    }

    private void CheckLevelComplete()
    {
        var lvl = levels[levelIndex];
        for (int i = 0; i < lvl.objectives.Count; i++)
            if (!objectiveCompleted[i]) return;

        CompleteLevel();
    }

    private void CompleteLevel()
    {
        levelRunning = false;
        score += Mathf.Max(0, levels[levelIndex].bonusOnLevelComplete);
        if (hud != null) hud.ShowMessage("Niveau réussi !");

        int next = levelIndex + 1;
        if (next >= levels.Count)
        {
            if (hud != null) hud.ShowMessage("GG ! Tous les niveaux terminés.");
            UpdateHUD();
            return;
        }

        SetupLevel(next);
        StartLevel();
    }

    private void FailLevel()
    {
        levelRunning = false;
        if (hud != null) hud.ShowMessage("Temps écoulé… Niveau recommencé.");
        SetupLevel(levelIndex);
        StartLevel();
    }

    private void SyncObjectivesFromGlobalStats()
    {
        var lvl = levels[levelIndex];

        for (int i = 0; i < lvl.objectives.Count; i++)
        {
            var o = lvl.objectives[i];

            int progress = 0;

            switch (o.type)
            {
                case ObjectiveType.SpawnAny:
                    progress = totalSpawned;
                    break;

                case ObjectiveType.ViewInfoAny:
                    progress = totalInfoViewed;
                    break;

                case ObjectiveType.DestroyAny:
                    progress = totalDestroyed;
                    break;

                case ObjectiveType.SpawnSpecific:
                    progress = (!string.IsNullOrEmpty(o.planetId) && spawnedPlanets.Contains(o.planetId)) ? 1 : 0;
                    break;

                case ObjectiveType.ViewInfoSpecific:
                    progress = (!string.IsNullOrEmpty(o.planetId) && infoPlanets.Contains(o.planetId)) ? 1 : 0;
                    break;

                case ObjectiveType.DestroySpecific:
                    progress = (!string.IsNullOrEmpty(o.planetId) && destroyedPlanets.Contains(o.planetId)) ? 1 : 0;
                    break;
            }

            objectiveProgress[i] = Mathf.Min(o.targetCount, progress);
            objectiveCompleted[i] = objectiveProgress[i] >= o.targetCount;
        }
    }

}

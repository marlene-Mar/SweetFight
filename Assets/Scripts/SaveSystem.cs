using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public Data gameData; // ScriptableObject asignado en Inspector

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // ══════════════════════════════════════════
    //  GUARDAR
    // ══════════════════════════════════════════
    public void Save()
    {
        GameManager.Instance?.SaveToData();
        CombatManager.Instance?.SaveCombatStats();

        // ✅ Renombrada a "snapshot" para evitar conflicto con el campo
        SaveData snapshot = new SaveData
        {
            playerHealth = gameData.playerHealth,
            playerMaxHealth = gameData.playerMaxHealth,
            playerPositionX = gameData.playerPositionX,
            playerPositionY = gameData.playerPositionY,
            playerPositionZ = gameData.playerPositionZ,

            camemiHealth = gameData.camemiHealth,
            camemiDefeated = gameData.camemiDefeated,
            camemiDialogueShown = gameData.camemiDialogueShown,

            currentCandies = gameData.currentCandies,
            maxCandies = gameData.maxCandies,
            guardianAllyCount = gameData.guardianAllyCount,
            guardians = gameData.guardians,   // ✅ guardianes

            flanCount = gameData.flanCount,

            lastCombatPlayerHits = gameData.lastCombatPlayerHits,
            lastCombatEnemyHits = gameData.lastCombatEnemyHits,
            lastCombatDamageDealt = gameData.lastCombatDamageDealt,
            lastCombatDamageTaken = gameData.lastCombatDamageTaken,
            lastCombatTimeRemaining = gameData.lastCombatTimeRemaining,

            lastScene = gameData.lastScene,
            isGameOver = gameData.isGameOver,

            masterVolume = gameData.masterVolume,
            musicVolume = gameData.musicVolume,
            sfxVolume = gameData.sfxVolume
        };

        string json = JsonUtility.ToJson(snapshot, prettyPrint: true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"[SaveSystem] Partida guardada en: {SavePath}");
    }

    // ══════════════════════════════════════════
    //  CARGAR
    // ══════════════════════════════════════════
    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveSystem] No existe archivo de guardado.");
            return;
        }

        // 1. Leer JSON y deserializar
        string json = File.ReadAllText(SavePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // 2. Volcar SaveData → ScriptableObject
        gameData.playerHealth = saveData.playerHealth;
        gameData.playerMaxHealth = saveData.playerMaxHealth;
        gameData.playerPositionX = saveData.playerPositionX;
        gameData.playerPositionY = saveData.playerPositionY;
        gameData.playerPositionZ = saveData.playerPositionZ;

        gameData.camemiHealth = saveData.camemiHealth;
        gameData.camemiDefeated = saveData.camemiDefeated;
        gameData.camemiDialogueShown = saveData.camemiDialogueShown;

        gameData.currentCandies = saveData.currentCandies;
        gameData.maxCandies = saveData.maxCandies;
        gameData.guardians = saveData.guardians;
        gameData.guardianAllyCount = saveData.guardianAllyCount;

        gameData.flanCount = saveData.flanCount;

        gameData.lastCombatPlayerHits = saveData.lastCombatPlayerHits;
        gameData.lastCombatEnemyHits = saveData.lastCombatEnemyHits;
        gameData.lastCombatDamageDealt = saveData.lastCombatDamageDealt;
        gameData.lastCombatDamageTaken = saveData.lastCombatDamageTaken;
        gameData.lastCombatTimeRemaining = saveData.lastCombatTimeRemaining;

        gameData.lastScene = saveData.lastScene;
        gameData.isGameOver = saveData.isGameOver;

        gameData.masterVolume = saveData.masterVolume;
        gameData.musicVolume = saveData.musicVolume;
        gameData.sfxVolume = saveData.sfxVolume;

        // 3. Cargar escena y aplicar datos
        if (!string.IsNullOrEmpty(saveData.lastScene))
            SceneManager.LoadScene(saveData.lastScene);

        GameManager.Instance?.LoadFromData();

        Debug.Log("[SaveSystem] Partida cargada.");
    }

    // ══════════════════════════════════════════
    //  UTILIDADES
    // ══════════════════════════════════════════
    public bool SaveExists() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveSystem] Archivo de guardado eliminado.");
        }
    }
}
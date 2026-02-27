[System.Serializable]
public class SaveData
{
    // Player
    public int playerHealth;
    public int playerMaxHealth;
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;

    // Camemi
    public int camemiHealth;
    public bool camemiDefeated;
    public bool camemiDialogueShown;

    // Guardian
    public int guardianHealth;
    public Data.GuardianSaveData[] guardians = new Data.GuardianSaveData[3];

    // Game State
    public int currentCandies;
    public int maxCandies;
    public int guardianAllyCount;

    // Inventory
    public int flanCount;

    // Combat Stats
    public int lastCombatPlayerHits;
    public int lastCombatEnemyHits;
    public int lastCombatDamageDealt;
    public int lastCombatDamageTaken;
    public float lastCombatTimeRemaining;

    // Scene
    public string lastScene;
    public bool isGameOver;

    // Audio
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
}
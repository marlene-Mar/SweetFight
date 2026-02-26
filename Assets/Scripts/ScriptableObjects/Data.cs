using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Data")]
public class Data : ScriptableObject
{
    // ==============================
    // Player Data
    // ==============================
    public int playerHealth;
    public int playerMaxHealth;        // vidaMaxima puede cambiar en el futuro
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;

    // ==============================
    // Camemi Data  (boss principal)
    // ==============================
    public int camemiHealth;
    public bool camemiDefeated;        // para no reiniciar el boss si ya murió
    public bool camemiDialogueShown;   // evita repetir el diálogo de encuentro

    // ==============================
    // Guardian Data 
    // ==============================
    public int guardianHealth;

    // Estado de cada guardián (índice 0, 1, 2)
    public GuardianSaveData[] guardians = new GuardianSaveData[3];

    [System.Serializable]
    public class GuardianSaveData
    {
        public float posX, posY, posZ;   // posición actual
        public bool isAlly;              // ¿es aliado en este momento?
        public float allyTimeRemaining;  // tiempo restante como aliado
        public bool onCooldown;          // ¿está en cooldown post-aliado?
    }

    // ==============================
    // Game State Data
    // ==============================
    public int currentCandies;         // renombrado desde currentPlayerInventory, más claro
    public int maxCandies;             // por si cambia según progreso
    public int guardianAllyCount;      // currrentGuardianAllies (corrige typo)

    // ==============================
    // Combat Stats  (útil para pantalla de resultados)
    // ==============================
    public int lastCombatPlayerHits;
    public int lastCombatEnemyHits;
    public int lastCombatDamageDealt;
    public int lastCombatDamageTaken;
    public float lastCombatTimeRemaining; // cuánto tiempo sobró

    // ==============================
    // Scene / Checkpoint
    // ==============================
    public string lastScene;           // nombre de la escena guardada
    public bool isGameOver;

    // ==============================
    // Audio Settings  (persisten entre sesiones)
    // ==============================
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;

    // ==============================
    // Inventory Data
    // ==============================
    public int flanCount; // cantidad de flanes en inventario
}
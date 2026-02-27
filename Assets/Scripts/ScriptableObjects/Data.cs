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
    // Camemi Data  
    // ==============================
    public int camemiHealth;
    public bool camemiDefeated;        // para no reiniciar el boss si ya murio
    public bool camemiDialogueShown;   // evita repetir el dialogo de encuentro

    // ==============================
    // Guardian Data 
    // ==============================
    public int guardianHealth;

    // Estado de cada guardian 
    public GuardianSaveData[] guardians = new GuardianSaveData[3];

    [System.Serializable]
    public class GuardianSaveData
    {
        public float posX, posY, posZ;   // posicionn actual
        public bool isAlly;              // Es aliado en este momento?
        public float allyTimeRemaining;  // tiempo restante como aliado
        public bool onCooldown;          
    }

    // ==============================
    // Game State Data
    // ==============================
    public int currentCandies;         // renombrado desde currentPlayerInventory, mas claro
    public int maxCandies;             // por si cambia segun progreso
    public int guardianAllyCount;      // currrentGuardianAllies (corrige typo)

    // ==============================
    // Combat Stats 
    // ==============================
    public int lastCombatPlayerHits;
    public int lastCombatEnemyHits;
    public int lastCombatDamageDealt;
    public int lastCombatDamageTaken;
    public float lastCombatTimeRemaining; 

    // ==============================
    // Scene / Checkpointw
    // ==============================
    public string lastScene;          
    public bool isGameOver;

    // ==============================
    // Audio Settings  
    // ==============================
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;

    // ==============================
    // Inventory Data
    // ==============================
    public int flanCount; // cantidad de flanes en inventario
}
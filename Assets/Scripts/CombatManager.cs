using UnityEngine;
using System.Collections;

// Maneja la lógica de combate, incluyendo el temporizador, el registro de golpes y la UI específica para los combates contra guardianes y Camemi
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("UI de Combate")]
    public GameObject combatUI;
    public GameObject combatCamemiUI;
    public UIManager uiManager;

    [Header("Duraciones de Combate")]
    [SerializeField] private float guardianCombatDuration = 40f;
    [SerializeField] private float camemiCombatDuration = 60f;

    // Estado interno
    private PompompurinController player;
    private VidaJugador vidaJugador;

    private int playerHitsLanded;
    private int enemyHitsLanded;
    private int totalDamageDealt;
    private int totalDamageTaken;

    private float combatTimer;
    private bool timerRunning;

    private GuardianController currentEnemy;
    private CamemiController currentCamemi;
    private bool isCamemiCombat;

    public bool IsInCombat => timerRunning;

    // ── Ciclo de vida ──────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (combatUI != null) combatUI.SetActive(false);
        if (combatCamemiUI != null) combatCamemiUI.SetActive(false);
    }

    // Inicia un combate contra un guardián específico
    public void StartGuardianCombat(GuardianController guardian, PompompurinController pompompurin)
    {
        Debug.Log($"StartGuardianCombat — timerRunning: {timerRunning}");  
        if (timerRunning) { Debug.LogWarning("CombatManager: Ya hay un combate activo."); return; } // Verificar si ya hay un combate activo

        isCamemiCombat = false;
        currentEnemy = guardian;
        player = pompompurin;
        vidaJugador = player.GetComponent<VidaJugador>();

        ResetStats(); // Reiniciar estadísticas de combate
        
        // Activar UI de combate
        if (combatUI != null) combatUI.SetActive(true);

        // Detener el movimiento del guardián al iniciar el combate
        var nav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null) nav.isStopped = true;

        player.inCombat = true;
        Debug.Log("¡Combate contra Guardián iniciado!");
        StartCoroutine(CombatTimerRoutine(guardianCombatDuration)); // Iniciar el temporizador del combate
        UIManager.Instance?.ShowTimer(); // Mostrar el temporizador en la UI
    }

    // Inicia un combate contra Camemi
    public void StartCamemiCombat(CamemiController camemi, PompompurinController pompompurin)
    {
        if (timerRunning) { Debug.LogWarning("CombatManager: Ya hay un combate activo."); return; }

        isCamemiCombat = true;
        currentCamemi = camemi;
        player = pompompurin;
        vidaJugador = player.GetComponent<VidaJugador>();

        ResetStats();

        if (combatCamemiUI != null) combatCamemiUI.SetActive(true);

        player.inCombat = true;
        Debug.Log("¡Combate contra Camemi iniciado!");
        StartCoroutine(CombatTimerRoutine(camemiCombatDuration));
        UIManager.Instance?.ShowTimerCamemi();
    }

    void ResetStats()
    {
        playerHitsLanded = enemyHitsLanded = totalDamageDealt = totalDamageTaken = 0;
    }

    // ── Registro de golpes ─────────────────────────

    public void OnPlayerHit(int damage)
    {
        if (!timerRunning) return;

        playerHitsLanded++;
        totalDamageDealt += damage;

        if (isCamemiCombat)
            currentCamemi?.TakeDamage(damage);

        Debug.Log($"[Jugador] golpeó → daño: {damage} | total infligido: {totalDamageDealt}");
    }

    public void OnPlayerHitGuardian(int damage)
    {
        if (!timerRunning) return;

        playerHitsLanded++;
        totalDamageDealt += damage;

        Debug.Log($"[Jugador→Guardián] daño: {damage} | total infligido: {totalDamageDealt}");
    }

    public void OnGuardianHit(int damage)
    {
        if (!timerRunning) return;

        enemyHitsLanded++;
        totalDamageTaken += damage;
        vidaJugador?.RecibirDaño(damage);

        Debug.Log($"[Guardián] golpeó → daño: {damage} | total recibido: {totalDamageTaken}");
    }

    /// <summary>Llamado cuando Camemi golpea al jugador.</summary>
    public void OnCamemiHit(int damage)
    {
        if (!timerRunning) return;

        enemyHitsLanded++;
        totalDamageTaken += damage;
        vidaJugador?.RecibirDaño(damage);

        Debug.Log($"[Camemi] golpeó → daño: {damage} | total recibido: {totalDamageTaken}");
    }

    // ── Fin de combate ─────────────────────────────

    public void EndCombat(bool playerWon)
    {
        if (!timerRunning) return;
        SaveCombatStats();

        StopAllCoroutines();
        timerRunning = false;

        if (combatUI != null) combatUI.SetActive(false);
        if (combatCamemiUI != null) combatCamemiUI.SetActive(false);

        player?.ExitCombat();

        if (isCamemiCombat)
        {
            UIManager.Instance?.HideTimerCamemi();

            if (playerWon)
                Debug.Log("¡Victoria contra Camemi!");
            else
            {
                Debug.Log("Tiempo agotado en combate Camemi.");
                currentCamemi?.OnCombatTimeOut();
            }
        }
        else
        {
            UIManager.Instance?.HideTimer();

            if (playerWon)
                Debug.Log("¡Victoria contra Guardián!");
            else
                currentEnemy?.EndCombat();
        }
    }

    // ── Temporizador ───────────────────────────────

    private IEnumerator CombatTimerRoutine(float duration)
    {
        timerRunning = true;
        combatTimer = duration;

        while (combatTimer > 0f)
        {
            combatTimer -= Time.deltaTime;

            if (isCamemiCombat)
                UIManager.Instance?.UpdateTimerCamemi(combatTimer);
            else
                UIManager.Instance?.UpdateTimer(combatTimer);

            yield return null;
        }

        if (isCamemiCombat)
            UIManager.Instance?.UpdateTimerCamemi(0f);
        else
            UIManager.Instance?.UpdateTimer(0f);

        Debug.Log("Tiempo de combate agotado.");
        EndCombat(false);
    }

    // ── Getters ────────────────────────────────────

    public int GetPlayerHits() => playerHitsLanded;
    public int GetEnemyHits() => enemyHitsLanded;
    public int GetTotalDamageDealt() => totalDamageDealt;
    public int GetTotalDamageTaken() => totalDamageTaken;

    [Header("Save Data")]
    public Data gameData;

    public void SaveCombatStats()
    {
        gameData.lastCombatPlayerHits = playerHitsLanded;
        gameData.lastCombatEnemyHits = enemyHitsLanded;
        gameData.lastCombatDamageDealt = totalDamageDealt;
        gameData.lastCombatDamageTaken = totalDamageTaken;
        gameData.lastCombatTimeRemaining = combatTimer;
    }
}
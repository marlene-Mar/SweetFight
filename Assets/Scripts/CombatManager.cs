using UnityEngine;
using System.Collections;

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

    // ── Inicio de combate ──────────────────────────

    public void StartGuardianCombat(GuardianController guardian, PompompurinController pompompurin)
    {
        if (timerRunning) { Debug.LogWarning("CombatManager: Ya hay un combate activo."); return; }

        isCamemiCombat = false;
        currentEnemy = guardian;
        player = pompompurin;
        vidaJugador = player.GetComponent<VidaJugador>();

        ResetStats();

        if (combatUI != null) combatUI.SetActive(true);

        var nav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null) nav.isStopped = true;

        player.inCombat = true;
        Debug.Log("¡Combate contra Guardián iniciado!");
        StartCoroutine(CombatTimerRoutine(guardianCombatDuration));
    }

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
    }

    void ResetStats()
    {
        playerHitsLanded = enemyHitsLanded = totalDamageDealt = totalDamageTaken = 0;
    }

    // ── Registro de golpes ─────────────────────────

    /// <summary>Llamado cuando Pompompurin golpea al enemigo.</summary>
    public void OnPlayerHit(int damage)
    {
        if (!timerRunning) return;

        playerHitsLanded++;
        totalDamageDealt += damage;

        if (isCamemiCombat)
            currentCamemi?.TakeDamage(damage);
        else
            currentEnemy?.TakeDamage(damage);

        Debug.Log($"[Jugador] golpeó → daño: {damage} | total infligido: {totalDamageDealt}");
    }

    /// <summary>Llamado cuando el Guardián golpea al jugador.</summary>
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
        // CORRECCIÓN CLAVE: usar timerRunning en lugar de player.inCombat
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

        StopAllCoroutines();
        timerRunning = false;

        if (combatUI != null) combatUI.SetActive(false);
        if (combatCamemiUI != null) combatCamemiUI.SetActive(false);

        player?.ExitCombat();

        if (isCamemiCombat)
        {
            if (playerWon)
            {
                Debug.Log("¡Victoria contra Camemi!");
            }
            else
            {
                // Tiempo agotado: Camemi muestra diálogo y vuelve a patrullar
                Debug.Log("Tiempo agotado en combate Camemi.");
                currentCamemi?.OnCombatTimeOut();
            }
        }
        else
        {
            if (playerWon)
                Debug.Log("¡Victoria contra Guardián!");
            else
                currentEnemy?.EndCombat();
        }
    }

    // ── Temporizador ───────────────────────────────

    private IEnumerator CombatTimerRoutine(float duration)
    {
        timerRunning = true;   // Se activa ANTES del primer yield
        combatTimer = duration;

        while (combatTimer > 0f)
        {
            combatTimer -= Time.deltaTime;
            UIManager.Instance?.UpdateTimer(combatTimer);
            yield return null;
        }

        UIManager.Instance?.UpdateTimer(0f);
        Debug.Log("Tiempo de combate agotado.");
        EndCombat(false);
    }

    // ── Getters ────────────────────────────────────

    public int GetPlayerHits() => playerHitsLanded;
    public int GetEnemyHits() => enemyHitsLanded;
    public int GetTotalDamageDealt() => totalDamageDealt;
    public int GetTotalDamageTaken() => totalDamageTaken;
}
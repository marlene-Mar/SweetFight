using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Singleton ligero (solo para esta escena)
    // ─────────────────────────────────────────────
    public static CombatManager Instance { get; private set; }

    [Header("Configuración de Combate")]
    public GameObject combatUI;
    public UIManager uiManager;

    [Header("Temporizador")]
    [SerializeField] private float combatDuration = 40f;

    // Estado interno
    private GuardianController currentEnemy;
    private PompompurinController player;
    private VidaJugador vidaJugador;

    private int playerHitsLanded;
    private int guardianHitsLanded;
    private int totalDamageDealt;
    private int totalDamageTaken;

    private float combatTimer;
    private bool timerRunning;
    public bool IsInCombat => timerRunning;

    // ─────────────────────────────────────────────
    //  Ciclo de vida
    // ─────────────────────────────────────────────
    void Awake()
    {
        // Singleton sencillo
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (combatUI != null) combatUI.SetActive(false);
    }

    // ─────────────────────────────────────────────
    //  Inicio de combate
    // ─────────────────────────────────────────────
    public void StartGuardianCombat(GuardianController guardian, PompompurinController pompompurin)
    {
        // Evitar iniciar un segundo combate si ya hay uno activo
        if (timerRunning)
        {
            Debug.LogWarning("CombatManager: Ya hay un combate activo, se ignora el nuevo.");
            return;
        }

        currentEnemy = guardian;
        player = pompompurin;
        vidaJugador = player.GetComponent<VidaJugador>();

        playerHitsLanded = guardianHitsLanded = totalDamageDealt = totalDamageTaken = 0;

        if (combatUI != null) combatUI.SetActive(true);

        // Detener al guardián en su posición de combate
        var guardianNav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (guardianNav != null) guardianNav.isStopped = true;

        player.inCombat = true;

        Debug.Log("¡Combate iniciado!");
        StartCoroutine(CombatTimerRoutine());
    }

    public void StartCamemiCombat(CamemiController camemi, PompompurinController player)
    {
        Debug.Log("Inicia combate Camemi");

        // Evitar iniciar un segundo combate si ya hay uno activo
        if (timerRunning)
        {
            Debug.LogWarning("CombatManager: Ya hay un combate activo, se ignora el nuevo.");
            return;
        }

        //currentEnemy = camemi;
        //player = pompompurin;
        //vidaJugador = player.GetComponent<VidaJugador>();

        // Lógica específica de Camemi
        //camemi.EnableWeapon();
        // lógica diferente si quieres
    }

    // ─────────────────────────────────────────────
    //  Registro de golpes
    // ─────────────────────────────────────────────
    public void OnPlayerHit(int damage)
    {
        if (!timerRunning) return;

        playerHitsLanded++;
        totalDamageDealt += damage;
        currentEnemy?.TakeDamage(damage);

        Debug.Log($"Pompompurin golpeó → Daño: {damage} | Acumulado: {totalDamageDealt}");
    }

    public void OnGuardianHit(int damage)
    {
        if (!timerRunning) return;

        guardianHitsLanded++;
        totalDamageTaken += damage;
        vidaJugador?.RecibirDaño(damage);

        Debug.Log($"Guardián golpeó → Daño: {damage} | Recibido total: {totalDamageTaken}");
    }

    // ─────────────────────────────────────────────
    //  Fin de combate
    // ─────────────────────────────────────────────
    public void EndCombat(bool playerWon)
    {
        if (!timerRunning) return; 

        StopAllCoroutines();
        timerRunning = false;

        LogCombatStats();

        if (combatUI != null) combatUI.SetActive(false);

        // Avisar al jugador UNA sola vez
        player?.ExitCombat();

        if (playerWon)
        {
            Debug.Log("¡Victoria! El guardián se une al equipo.");
            // BecomeAlly se llama desde GuardianController.Die() con delay,
            // así que aquí NO lo llamamos para respetar la animación de muerte.
        }
        else
        {
            Debug.Log("Tiempo agotado. El guardián vuelve a patrullar.");
            currentEnemy?.EndCombat();
        }
    }

    // ─────────────────────────────────────────────
    //  Temporizador
    // ─────────────────────────────────────────────
    private IEnumerator CombatTimerRoutine()
    {
        timerRunning = true;
        combatTimer = combatDuration;

        while (combatTimer > 0f)
        {
            combatTimer -= Time.deltaTime;
            UIManager.Instance?.UpdateTimer(combatTimer);
            yield return null;
        }

        UIManager.Instance?.UpdateTimer(0f);
        // ← NO tocar timerRunning aquí, EndCombat lo maneja
        Debug.Log("Tiempo de combate agotado.");
        EndCombat(false); // EndCombat hace StopAllCoroutines + timerRunning = false
    }
    // ─────────────────────────────────────────────
    //  Estadísticas
    // ─────────────────────────────────────────────
    private void LogCombatStats()
    {
        Debug.Log("=== ESTADÍSTICAS DEL COMBATE ===");
        Debug.Log($"Golpes de Pompompurin : {playerHitsLanded} | Daño total: {totalDamageDealt}");
        Debug.Log($"Golpes del Guardián   : {guardianHitsLanded} | Daño total: {totalDamageTaken}");
        Debug.Log("================================");
    }

    

    // Getters opcionales para UI externa
    public int GetPlayerHits() => playerHitsLanded;
    public int GetGuardianHits() => guardianHitsLanded;
    public int GetTotalDamageDealt() => totalDamageDealt;
    public int GetTotalDamageTaken() => totalDamageTaken;
}
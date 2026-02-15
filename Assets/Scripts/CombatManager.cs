using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    [Header("Configuración de Combate")]
    public GameObject combatUI;
    public UIManager uiManager;
    private GuardianController currentEnemy;
    private PompompurinController player;
    private int playerHitsLanded = 0;
    private int guardianHitsLanded = 0;
    private int totalDamageDealt = 0;
    private int totalDamageTaken = 0;

    private float combatTimer = 40f;
    private bool timerRunning = false;

    void Start()
    {
        if (combatUI != null)
            combatUI.SetActive(false);
    }

    public void StartCombat(GuardianController guardian, PompompurinController pompompurin)
    {
        currentEnemy = guardian;
        player = pompompurin;
        playerHitsLanded = 0;
        guardianHitsLanded = 0;
        totalDamageDealt = 0;
        totalDamageTaken = 0;

        Debug.Log("¡Combate iniciado con el Guardian!");

        if (combatUI != null)
            combatUI.SetActive(true);

        InitializeCombat();

        StartCoroutine(CombatTimer());
    }

    void InitializeCombat()
    {
        Debug.Log("Inicializando combate...");
        if (currentEnemy != null)
        {
            UnityEngine.AI.NavMeshAgent guardianNav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (guardianNav != null)
                guardianNav.isStopped = true;
        }
        if (player != null)
        {
            player.inCombat = true;
            // player.SetInCombat(true);  // FIX: Usa método público en lugar de acceso directo
        }
    }

    public void OnPlayerHit(int damage)
    {
        playerHitsLanded++;
        totalDamageDealt += damage;

        Debug.Log($"¡Pompompurin conectó golpe #{playerHitsLanded}! Daño: {damage} | Total: {totalDamageDealt}");

        if (currentEnemy != null)
        {
            currentEnemy.TakeDamage(damage);
        }
    }

    public void OnGuardianHit(int damage)
    {
        guardianHitsLanded++;
        totalDamageTaken += damage;

        Debug.Log($"¡Guardián conectó golpe #{guardianHitsLanded}! Daño: {damage} | Total recibido: {totalDamageTaken}");

        if (player != null)
        {
            //player.TakeDamage(damage);
        }
    }

    public void EndCombat(bool playerWon)
    {
        if (timerRunning)
            StopAllCoroutines();

        timerRunning = false;

        Debug.Log(playerWon ? "¡Victoria!" : "Derrota...");

        ShowCombatStats();

        if (combatUI != null)
            combatUI.SetActive(false);

        // Reset jugador
        if (player != null)
            player.ExitCombat();

        // Reset guardián
        if (currentEnemy != null)
        {
            // Reactivar NavMeshAgent
            var nav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null)
            {
                nav.isStopped = false;
            }

            //// Reset flag de combate en guardián si existe
            //currentEnemy.GuardianState.Combat = false;

            // Llamar a método de patrullaje
            currentEnemy.PatrolBehaviour();
        }
    }

    void OnPlayerVictory()
    {
        if (player != null)
        {
            player.ExitCombat();
        }
        if (currentEnemy != null)
        {
            currentEnemy.BecomeAlly();
        }
        Debug.Log("¡Has derrotado al Guardian! Ahora te ayudará durante 1 minuto.");
    }

    void ShowCombatStats()
    {
        Debug.Log("=== ESTADÍSTICAS DEL COMBATE ===");
        Debug.Log($"Golpes de Pompompurin: {playerHitsLanded}");
        Debug.Log($"Daño total infligido: {totalDamageDealt}");
        Debug.Log($"Golpes del Guardián: {guardianHitsLanded}");
        Debug.Log($"Daño total recibido: {totalDamageTaken}");
        Debug.Log("================================");
    }


    // FIX: Resetea jugador + Guardian (agrega ResumePatrol() si existe en GuardianController)
    void OnPlayerDefeat()
    {
        Debug.Log("Has sido derrotado por el Guardian.");
        if (player != null)
        {
            player.ExitCombat();
        }
        if (currentEnemy != null)
        {
            UnityEngine.AI.NavMeshAgent nav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.isStopped = false;
        }
        // Lógica Game Over aquí
    }
    public int GetPlayerHits() => playerHitsLanded;
    public int GetGuardianHits() => guardianHitsLanded;
    public int GetTotalDamageDealt() => totalDamageDealt;
    public int GetTotalDamageTaken() => totalDamageTaken;

    private IEnumerator CombatTimer()
    {
        timerRunning = true;
        combatTimer = 40f;

        while (combatTimer > 0f)
        {
            combatTimer -= Time.deltaTime; // o Time.unscaledDeltaTime si pausas el juego
            UIManager.Instance.UpdateTimer(combatTimer);
            yield return null;
        }

        timerRunning = false;

        Debug.Log("Tiempo terminado, finaliza combate");
        EndCombat(false); // termina el combate automáticamente
    }
}

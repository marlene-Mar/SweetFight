using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Configuración de Combate")]
    public GameObject combatUI;

    private GuardianController currentEnemy;
    private PompompurinController player;

    private int playerHitsLanded = 0;
    private int guardianHitsLanded = 0;
    private int totalDamageDealt = 0;

    void Start()
    {
        // Ocultar UI de combate al inicio
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

        Debug.Log("¡Combate iniciado con el Guardian!");

        // Mostrar UI de combate
        if (combatUI != null)
            combatUI.SetActive(true);

        InitializeCombat();
    }

    void InitializeCombat()
    {
        Debug.Log("Inicializando combate...");

        // Detener el NavMesh del Guardian
        if (currentEnemy != null)
        {
            UnityEngine.AI.NavMeshAgent guardianNav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (guardianNav != null)
                guardianNav.isStopped = true;
        }

        // Activar el modo de combate en Pompompurin
        if (player != null)
        {
            player.inCombat = true;
        }
    }

    public void OnPlayerHit(int damage)
    {
        playerHitsLanded++;
        totalDamageDealt += damage;

        Debug.Log($"¡Pompompurin conectó golpe #{playerHitsLanded}! Daño total: {totalDamageDealt}");

        // Aquí puedes actualizar UI de combate
        // Por ejemplo: UpdateCombatUI();
    }

    public void OnGuardianHit()
    {
        guardianHitsLanded++;

        Debug.Log($"¡Guardián conectó golpe #{guardianHitsLanded}!");

        // Aquí puedes actualizar UI o efectos visuales
    }

    public void EndCombat(bool playerWon)
    {
        Debug.Log(playerWon ? "¡Victoria!" : "Derrota...");

        ShowCombatStats();

        // Ocultar UI de combate
        if (combatUI != null)
            combatUI.SetActive(false);

        // Lógica post-combate
        if (playerWon)
        {
            OnPlayerVictory();
        }
        else
        {
            OnPlayerDefeat();
        }
    }

    void ShowCombatStats()
    {
        Debug.Log("=== ESTADÍSTICAS DEL COMBATE ===");
        Debug.Log($"Golpes de Pompompurin: {playerHitsLanded}");
        Debug.Log($"Daño total infligido: {totalDamageDealt}");
        Debug.Log($"Golpes del Guardián: {guardianHitsLanded}");
        Debug.Log("================================");
    }

    void OnPlayerVictory()
    {
        // Desactivar modo combate en Pompompurin
        if (player != null)
        {
            player.ExitCombat();
        }

        // Desactivar combate del guardián
        if (currentEnemy != null)
        {
            currentEnemy.EndCombat();
        }

        Debug.Log("El Guardian ha sido derrotado. Ahora te ayudará en tu misión.");
    }

    void OnPlayerDefeat()
    {
        Debug.Log("Has sido derrotado por el Guardian.");

        // - Recargar escena
        // - Mostrar pantalla de Game Over
        // - Respawn del jugador
    }

    public int GetPlayerHits()
    {
        return playerHitsLanded;
    }

    public int GetGuardianHits()
    {
        return guardianHitsLanded;
    }

    public int GetTotalDamage()
    {
        return totalDamageDealt;
    }
}
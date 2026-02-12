using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Configuración de Combate")]
    public GameObject combatUI;

    private GuardianController currentEnemy;
    private PompompurinController player;

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

    public void EndCombat(bool playerWon)
    {
        Debug.Log(playerWon ? "¡Victoria!" : "Derrota...");

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

        // Aquí puedes agregar lógica de reinicio
        // Por ejemplo:
        // - Recargar escena
        // - Mostrar pantalla de Game Over
        // - Respawn del jugador
    }
}
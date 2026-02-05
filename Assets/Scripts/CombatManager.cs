using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Configuración de Combate")]
    public GameObject combatUI;

    private GuardianController currentEnemy;

    void Start()
    {
        // Ocultar UI de combate al inicio
        if (combatUI != null)
            combatUI.SetActive(false);
    }

    public void StartCombat(GuardianController guardian)
    {
        currentEnemy = guardian;

        Debug.Log("¡Combate iniciado con el Guardian!");

        // Mostrar UI de combate
        if (combatUI != null)
            combatUI.SetActive(true);

        // Aquí puedes agregar tu lógica de combate
        // Por ejemplo:
        // - Activar sistema de turnos
        // - Mostrar barras de vida
        // - Habilitar botones de ataque/defensa
        // - Etc.

        InitializeCombat();
    }

    void InitializeCombat()
    {
        // Configuración inicial del combate
        Debug.Log("Inicializando combate...");

        // Detener el NavMesh del Guardian
        if (currentEnemy != null)
        {
            UnityEngine.AI.NavMeshAgent guardianNav = currentEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (guardianNav != null)
                guardianNav.isStopped = true;
        }

        // Activar el modo de combate en Pompompurin
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PompompurinController pompompurin = player.GetComponent<PompompurinController>();
            if (pompompurin != null)
            {
                pompompurin.inCombat = true;
                pompompurin.enabled = true; // Asegurar que esté habilitado para el combate
            }
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PompompurinController pompompurin = player.GetComponent<PompompurinController>();
            if (pompompurin != null)
            {
                pompompurin.inCombat = false;
            }
        }

        // Aquí puedes:
        // - Dar recompensas
        // - Desbloquear siguiente misión
        // - Hacer que el Guardian se una al jugador
        Debug.Log("El Guardian ha sido derrotado. Ahora te ayudará en tu misión.");
    }

    void OnPlayerDefeat()
    {
        // Aquí puedes:
        // - Reiniciar el encuentro
        // - Mandar al jugador a un punto de respawn
        // - Mostrar pantalla de Game Over
        Debug.Log("Has sido derrotado por el Guardian.");
    }
}
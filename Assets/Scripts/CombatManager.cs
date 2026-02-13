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
    private int totalDamageTaken = 0;

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
        }
    }

    // Llamado cuando Pompompurin golpea al Guardian
    public void OnPlayerHit(int damage)
    {
        playerHitsLanded++;
        totalDamageDealt += damage;

        Debug.Log($"¡Pompompurin conectó golpe #{playerHitsLanded}! Daño: {damage} | Total: {totalDamageDealt}");

        // Aplicar daño al Guardian
        if (currentEnemy != null)
        {
            currentEnemy.TakeDamage(damage);
        }
    }

    // Llamado cuando el Guardian golpea a Pompompurin
    public void OnGuardianHit(int damage)
    {
        guardianHitsLanded++;
        totalDamageTaken += damage;

        Debug.Log($"¡Guardián conectó golpe #{guardianHitsLanded}! Daño: {damage} | Total recibido: {totalDamageTaken}");

        // Aplicar daño a Pompompurin
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }

    public void EndCombat(bool playerWon)
    {
        Debug.Log(playerWon ? "¡Victoria!" : "Derrota...");
        ShowCombatStats();

        if (combatUI != null)
            combatUI.SetActive(false);

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
        Debug.Log($"Daño total recibido: {totalDamageTaken}");
        Debug.Log("================================");
    }

    void OnPlayerVictory()
    {
        if (player != null)
        {
            player.ExitCombat();
        }

        if (currentEnemy != null)
        {
            currentEnemy.EndCombat();
        }

        Debug.Log("El Guardian ha sido derrotado. Ahora te ayudará en tu misión.");
    }

    void OnPlayerDefeat()
    {
        Debug.Log("Has sido derrotado por el Guardian.");
        // Aquí puedes agregar:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public int GetPlayerHits() => playerHitsLanded;
    public int GetGuardianHits() => guardianHitsLanded;
    public int GetTotalDamageDealt() => totalDamageDealt;
    public int GetTotalDamageTaken() => totalDamageTaken;
}
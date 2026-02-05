using UnityEngine;
using UnityEngine.AI;

public class GuardianController : MonoBehaviour
{
    [Header("Patrullaje")]
    public Transform[] patrolPoints; // 3 posiciones de patrullaje
    private int currentPatrolIndex = 0;
    public float patrolWaitTime = 2f;
    private float patrolTimer = 0f;

    [Header("Detección")]
    public float detectionRadius = 5f;
    public Transform player;
    private bool hasMetPlayer = false;

    [Header("Componentes")]
    private NavMeshAgent navAgent;
    private Animator animator;

    public enum GuardianState
    {
        Patrolling,
        Greeting,
        Dialogue,
        Combat
    }

    [Header("Estado Actual")]
    public GuardianState currentState = GuardianState.Patrolling;

    [Header("Animaciones")]
    public string walkAnimationName = "Walk";
    public string greetAnimationName = "Greet";
    public string attackAnimationName = "Attack";
    public string idleAnimationName = "Idle";

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Si no tienes referencia al jugador, búscalo
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Comenzar patrullaje
        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case GuardianState.Patrolling:
                UpdatePatrol();
                CheckPlayerDistance();
                break;

            case GuardianState.Greeting:
                // El estado de saludo se maneja por evento de animación
                break;

            case GuardianState.Dialogue:
                // El diálogo se maneja desde DialogueManager
                break;

            case GuardianState.Combat:
                // El combate se maneja desde CombatManager
                break;
        }

        UpdateAnimations();
    }

    void UpdatePatrol()
    {
        if (patrolPoints.Length == 0) return;

        // Verificar si llegó al punto de patrullaje
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
            {
                GoToNextPatrolPoint();
                patrolTimer = 0f;
            }
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void CheckPlayerDistance()
    {
        if (player == null || hasMetPlayer) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            OnPlayerDetected();
        }
    }

    void OnPlayerDetected()
    {
        hasMetPlayer = true;
        currentState = GuardianState.Greeting;

        // Detener movimiento
        navAgent.isStopped = true;

        // Mirar al jugador
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);

        // Ejecutar animación de saludo
        if (animator != null)
        {
            animator.SetTrigger(greetAnimationName);
        }

        // Esperar un momento y abrir diálogo
        Invoke(nameof(StartDialogue), 1.5f);
    }

    void StartDialogue()
    {
        currentState = GuardianState.Dialogue;

        // Notificar al DialogueManager
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.StartGuardianDialogue(this);
        }
    }

    public void StartCombat()
    {
        currentState = GuardianState.Combat;

        // Ejecutar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger(attackAnimationName);
        }

        // Notificar al sistema de combate
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            combatManager.StartCombat(this);
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Establecer velocidad para animación de caminar
        float speed = navAgent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        // Alternativamente, usar booleanos
        animator.SetBool("IsWalking", speed > 0.1f);
    }

    // Para visualizar el radio de detección en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

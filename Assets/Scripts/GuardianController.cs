//using UnityEngine;
//using UnityEngine.AI;

//public class GuardianController : MonoBehaviour
//{
//    private NavMeshAgent agent;
//    private Animator animator;
//    private MeshCollider[] validSurfaces;
//    private Transform player;
//    private PompompurinController playerController;
//    private DialogueManager dialogueManager;

//    public float patrolRadius = 25f;
//    public float waitTimeBetweenPoints = 1.5f;
//    public float detectionDistance = 5f;

//    private float waitTimer;
//    private bool hasDestination;

//    private enum GuardianState
//    {
//        Patrolling,
//        Greeting,
//        Talking,
//        Combat
//    }

//    private GuardianState currentState;

//    void Awake()
//    {
//        agent = GetComponent<NavMeshAgent>();
//        animator = GetComponent<Animator>();
//        dialogueManager = FindObjectOfType<DialogueManager>();
//    }

//    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
//    {
//        validSurfaces = surfaces;
//        player = playerTransform;

//        // Obtener el controlador del jugador
//        playerController = player.GetComponent<PompompurinController>();

//        currentState = GuardianState.Patrolling;
//        SetWalk(true);
//    }

//    void Update()
//    {
//        if (player == null) return;

//        switch (currentState)
//        {
//            case GuardianState.Patrolling:
//                PatrolBehaviour();
//                DetectPlayer();
//                break;

//            case GuardianState.Greeting:
//            case GuardianState.Talking:
//                // Mantener mirando al jugador durante el saludo y diálogo
//                LookAtPlayer();
//                break;

//            case GuardianState.Combat:
//                // En combate, seguir mirando al jugador
//                LookAtPlayer();
//                break;
//        }
//    }

//    void PatrolBehaviour()
//    {
//        if (!agent.pathPending && agent.remainingDistance < 0.5f)
//        {
//            SetWalk(false);
//            waitTimer += Time.deltaTime;

//            if (waitTimer >= waitTimeBetweenPoints)
//            {
//                MoveToRandomPoint();
//                waitTimer = 0;
//            }
//        }
//        else
//        {
//            SetWalk(true);
//        }
//    }

//    void MoveToRandomPoint()
//    {
//        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
//        randomDirection += transform.position;

//        NavMeshHit hit;
//        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
//        {
//            agent.SetDestination(hit.position);
//            hasDestination = true;
//        }
//    }

//    void DetectPlayer()
//    {
//        float distance = Vector3.Distance(transform.position, player.position);

//        if (distance <= detectionDistance)
//        {
//            StartGreeting();
//        }
//    }

//    void StartGreeting()
//    {
//        currentState = GuardianState.Greeting;
//        agent.isStopped = true;
//        SetWalk(false);

//        // Activar animación de saludo
//        animator.SetBool("isGreeting", true);

//        // **Detener al jugador para el diálogo**
//        if (playerController != null)
//        {
//            playerController.EnterDialogue();
//        }

//        Invoke(nameof(StartDialogue), 2f);
//    }

//    void StartDialogue()
//    {
//        animator.SetBool("isGreeting", false);
//        currentState = GuardianState.Talking;

//        if (dialogueManager != null)
//        {
//            dialogueManager.StartGuardianDialogue(this);
//        }
//    }

//    // Este método lo llama el DialogueManager cuando termina el diálogo
//    public void EndDialogue()
//    {
//        currentState = GuardianState.Combat;

//        // **Salir del diálogo e iniciar combate para el jugador**
//        if (playerController != null)
//        {
//            playerController.ExitDialogue();
//            playerController.StartCombatAfterDialogue();
//        }

//        StartCombat();
//    }

//    void StartCombat()
//    {
//        // Activar parámetro InCombat primero
//        animator.SetBool("InCombat", true);

//        // Luego disparar el trigger de golpe
//        animator.SetTrigger("GolpeP");
//    }

//    void LookAtPlayer()
//    {
//        if (player != null)
//        {
//            Vector3 lookDirection = player.position - transform.position;
//            lookDirection.y = 0; // Mantener en el plano horizontal

//            if (lookDirection != Vector3.zero)
//            {
//                transform.rotation = Quaternion.Slerp(
//                    transform.rotation,
//                    Quaternion.LookRotation(lookDirection),
//                    Time.deltaTime * 5f
//                );
//            }
//        }
//    }

//    void SetWalk(bool value)
//    {
//        animator.SetBool("Walk", value);
//    }
//}

using UnityEngine;
using UnityEngine.AI;

public class GuardianController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private MeshCollider[] validSurfaces;
    private Transform player;
    private PompompurinController playerController;
    private DialogueManager dialogueManager;
    private CombatManager combatManager;

    public float patrolRadius = 25f;
    public float waitTimeBetweenPoints = 1.5f;
    public float detectionDistance = 5f;

    // Configuración de combate
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    // Salud del guardián
    public int maxHealth = 100;
    private int currentHealth;

    private float waitTimer;
    private bool hasDestination;

    private enum GuardianState
    {
        Patrolling,
        Greeting,
        Talking,
        Combat
    }

    private GuardianState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        combatManager = FindObjectOfType<CombatManager>();

        currentHealth = maxHealth;
    }

    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
    {
        validSurfaces = surfaces;
        player = playerTransform;

        // Obtener el controlador del jugador
        playerController = player.GetComponent<PompompurinController>();

        currentState = GuardianState.Patrolling;
        SetWalk(true);
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case GuardianState.Patrolling:
                PatrolBehaviour();
                DetectPlayer();
                break;

            case GuardianState.Greeting:
            case GuardianState.Talking:
                LookAtPlayer();
                break;

            case GuardianState.Combat:
                LookAtPlayer();
                CombatBehaviour();
                break;
        }
    }

    void PatrolBehaviour()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetWalk(false);
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeBetweenPoints)
            {
                MoveToRandomPoint();
                waitTimer = 0;
            }
        }
        else
        {
            SetWalk(true);
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            hasDestination = true;
        }
    }

    void DetectPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionDistance)
        {
            StartGreeting();
        }
    }

    void StartGreeting()
    {
        currentState = GuardianState.Greeting;
        agent.isStopped = true;
        SetWalk(false);

        animator.SetBool("isGreeting", true);

        if (playerController != null)
        {
            playerController.EnterDialogue();
        }

        Invoke(nameof(StartDialogue), 2f);
    }

    void StartDialogue()
    {
        animator.SetBool("isGreeting", false);
        currentState = GuardianState.Talking;

        if (dialogueManager != null)
        {
            dialogueManager.StartGuardianDialogue(this);
        }
    }

    public void EndDialogue()
    {
        currentState = GuardianState.Combat;

        if (playerController != null)
        {
            playerController.ExitDialogue();
            playerController.StartCombatAfterDialogue();
        }

        // Iniciar combate a través del CombatManager
        if (combatManager != null)
        {
            combatManager.StartCombat(this, playerController);
        }

        StartCombat();
    }

    void StartCombat()
    {
        animator.SetBool("InCombat", true);
        attackTimer = 0f;
        ExecuteAttack();
    }

    void CombatBehaviour()
    {
        if (!isAttacking)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= timeBetweenAttacks)
            {
                ExecuteAttack();
                attackTimer = 0f;
            }
        }
    }

    void ExecuteAttack()
    {
        isAttacking = true;
        animator.SetTrigger("GolpeP");

        Invoke(nameof(EndAttack), 1f);
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    // **DETECCIÓN DE GOLPES DEL JUGADOR**
    void OnTriggerEnter(Collider other)
    {
        // Solo recibir daño si está en combate
        if (currentState != GuardianState.Combat) return;

        // Verificar si es la mano de Pompompurin
        if (other.CompareTag("PlayerWeapon") || other.name.Contains("mano"))
        {
            // Buscar al controlador del jugador
            PompompurinController pompom = other.GetComponentInParent<PompompurinController>();

            if (pompom != null)
            {
                int damage = pompom.GetCurrentDamage();
                TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Guardian recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");

        // Aquí puedes agregar animación de recibir golpe
        // animator.SetTrigger("TakeDamage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Guardian derrotado!");
        currentState = GuardianState.Patrolling;
        animator.SetBool("InCombat", false);

        // Notificar al CombatManager
        if (combatManager != null)
        {
            combatManager.EndCombat(true);
        }

        // Opcional: Desactivar el guardián o hacer que se una al jugador
        // gameObject.SetActive(false);
    }

    void LookAtPlayer()
    {
        if (player != null)
        {
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDirection),
                    Time.deltaTime * 5f
                );
            }
        }
    }

    void SetWalk(bool value)
    {
        animator.SetBool("Walk", value);
    }

    public void EndCombat()
    {
        currentState = GuardianState.Patrolling;
        animator.SetBool("InCombat", false);
        agent.isStopped = false;
        isAttacking = false;
        attackTimer = 0f;
    }
}
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GuardianController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private MeshCollider[] validSurfaces;
    private Transform player;
    private PompompurinController playerController;
    private DialogueManager dialogueManager;
    private CombatManager combatManager;

    //Patrulla
    public float patrolRadius = 25f;
    public float waitTimeBetweenPoints = 1.5f;
    public float detectionDistance = 5f;

    // Configuración de combate
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private bool canReceiveDamage = false; // NUEVO: Controlar cuándo puede recibir daño

    // Salud del guardián
    public int maxHealth = 100;
    private int currentHealth;

    public GameObject weaponObject;
    public Collider weaponCollider;
    public int weaponDamage = 15;

    // Configuración de aliado
    public float allyDuration = 60f;
    public float followDistance = 3f;
    public float allyDetectionRange = 10f;
    public LayerMask enemyLayer;
    private float allyTimer = 0f;
    private bool isAlly = false;

    private float waitTimer;
    private bool hasDestination;

    public bool playerNearby;

    private enum GuardianState
    {
        Patrolling,
        Greeting,
        Talking,
        Combat,
        Ally,
        AllyDefending
    }

    private GuardianState currentState;
    private Transform currentEnemyTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        combatManager = FindObjectOfType<CombatManager>();

        currentHealth = maxHealth;
    }

    void Start()
    {
        if (weaponCollider == null && weaponObject != null)
        {
            weaponCollider = weaponObject.GetComponent<Collider>();
        }

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;

            if (weaponCollider.GetComponent<GuardianWeaponCollider>() == null)
            {
                weaponCollider.gameObject.AddComponent<GuardianWeaponCollider>();
            }
        }

        // Asegurarse de que el arma esté desactivada al inicio
        if (weaponObject != null)
            weaponObject.SetActive(false);
    }

    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
    {
        validSurfaces = surfaces;
        player = playerTransform;
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

            case GuardianState.Ally:
                AllyBehaviour();
                break;

            case GuardianState.AllyDefending:
                DefendPlayerBehaviour();
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

        if (combatManager != null)
        {
            combatManager.StartCombat(this, playerController);
        }

        if (weaponObject != null)
            weaponObject.SetActive(true);

        StartCombat();
    }

    void StartCombat()
    {
        Debug.Log("Guardian: Iniciando combate");

        animator.SetBool("InCombat", true);
        canReceiveDamage = true; // ACTIVAR daño después de iniciar combate
        attackTimer = 0f;

        // Pequeño delay antes del primer ataque
        Invoke(nameof(ExecuteAttack), 0.5f);
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
        if (currentState != GuardianState.Combat && currentState != GuardianState.AllyDefending)
            return;

        isAttacking = true;
        animator.SetTrigger("GolpeP");

        Debug.Log("Guardian: Ejecutando ataque");

        Invoke(nameof(EndAttack), 1f);
    }

    void EndAttack()
    {
        isAttacking = false;
        Debug.Log("Guardian: Ataque terminado");
    }

    // COMPORTAMIENTO COMO ALIADO
    void AllyBehaviour()
    {
        allyTimer += Time.deltaTime;

        if (allyTimer >= allyDuration)
        {
            EndAllyMode();
            return;
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, allyDetectionRange, enemyLayer);

        if (enemies.Length > 0)
        {
            currentEnemyTarget = enemies[0].transform;
            currentState = GuardianState.AllyDefending;
            animator.SetBool("InCombat", true);

            if (weaponObject != null)
                weaponObject.SetActive(true);

            Debug.Log($"¡Guardian detectó enemigo: {currentEnemyTarget.name}! Entrando en combate.");
        }
        else
        {
            FollowPlayer();
        }
    }

    void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            SetWalk(true);
        }
        else
        {
            agent.isStopped = true;
            SetWalk(false);
            LookAtPlayer();
        }
    }

    void DefendPlayerBehaviour()
    {
        allyTimer += Time.deltaTime;

        if (allyTimer >= allyDuration)
        {
            EndAllyMode();
            return;
        }

        if (currentEnemyTarget == null)
        {
            currentState = GuardianState.Ally;
            animator.SetBool("InCombat", false);

            if (weaponObject != null)
                weaponObject.SetActive(false);

            return;
        }

        LookAtTarget(currentEnemyTarget);

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

    void LookAtTarget(Transform target)
    {
        if (target != null)
        {
            Vector3 lookDirection = target.position - transform.position;
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

    public void BecomeAlly()
    {
        isAlly = true;
        allyTimer = 0f;
        currentState = GuardianState.Ally;
        animator.SetBool("InCombat", false);
        animator.SetBool("Die", false); // Resetear animación de muerte
        agent.isStopped = false;
        canReceiveDamage = false; // Ya no recibe daño del jugador

        gameObject.tag = "Guardian";

        if (weaponObject != null)
            weaponObject.SetActive(false);

        Debug.Log($"¡Guardian se ha unido a tu equipo por {allyDuration} segundos!");

        StartCoroutine(ShowAllyMessage());
    }

    IEnumerator ShowAllyMessage()
    {
        Debug.Log("=== EL GUARDIAN ES AHORA TU ALIADO ===");
        Debug.Log("Te protegerá de enemigos durante 1 minuto.");
        yield return new WaitForSeconds(allyDuration - 10f);
        Debug.Log("¡El Guardian te abandonará en 10 segundos!");
        yield return new WaitForSeconds(10f);
    }

    void EndAllyMode()
    {
        isAlly = false;
        currentState = GuardianState.Patrolling;
        animator.SetBool("InCombat", false);

        if (weaponObject != null)
            weaponObject.SetActive(false);

        Debug.Log("El Guardian ha dejado de ser tu aliado y vuelve a patrullar.");

        currentHealth = maxHealth;
    }

    public void EnableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            Debug.Log("Guardian: Lanza activada");
        }
    }

    public void DisableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            Debug.Log("Guardian: Lanza desactivada");
        }
    }

    public void NotifyWeaponHit()
    {
        if (isAlly && currentState == GuardianState.AllyDefending)
        {
            Debug.Log("¡Guardian aliado golpeó a un enemigo!");
        }
        else if (!isAlly && combatManager != null)
        {
            combatManager.OnGuardianHit(weaponDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        // VERIFICAR si puede recibir daño
        if (!canReceiveDamage)
        {
            Debug.Log("Guardian: No puede recibir daño aún (no está en combate)");
            return;
        }

        if (currentState != GuardianState.Combat)
        {
            Debug.Log("Guardian: No puede recibir daño (no está en estado de combate)");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Guardian recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");

        // NO disparar trigger de golpe aquí, solo la animación de recibir daño si existe
        // animator.SetTrigger("RecibirGolpe"); // Agregar este trigger en el animator si existe

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Guardian derrotado!");
        canReceiveDamage = false;
        animator.SetBool("Die", true);
        animator.SetBool("InCombat", false);
        isAttacking = false;

        if (combatManager != null)
        {
            combatManager.EndCombat(true);
        }
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
        canReceiveDamage = false;

        if (weaponObject != null)
            weaponObject.SetActive(false);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsAlly()
    {
        return isAlly;
    }

    public bool CanReceiveDamage()
    {
        return canReceiveDamage;
    }
}

public class GuardianWeaponCollider : MonoBehaviour
{
    private GuardianController guardianController;

    void Start()
    {
        guardianController = GetComponentInParent<GuardianController>();

        if (guardianController == null)
        {
            Debug.LogError($"GuardianWeaponCollider en {gameObject.name} no encontró GuardianController en el padre!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (guardianController == null) return;

        if (guardianController.IsAlly())
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log($"¡Guardian aliado golpeó a enemigo: {other.name}!");
                guardianController.NotifyWeaponHit();
            }
        }
        else
        {
            if (other.CompareTag("Player") || other.name.Contains("Pompompurin"))
            {
                Debug.Log($"¡Lanza del Guardian golpeó a {other.name}!");
                guardianController.NotifyWeaponHit();
            }
        }
    }
}

//using UnityEngine;

//public class GuardianController : MonoBehaviour
//{
//    public int life = 100;

//    private MeshCollider[] validSurfaces;
//    private Transform player;

//    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
//    {
//        validSurfaces = surfaces;
//        player = playerTransform;

//        Debug.Log($"Guardian inicializado. Player asignado: {player != null}");
//    }

//    public void StartGreeting()
//    {
//        GameFlowManager.Instance.ChangeState(GameState.Greeting);
//    }

//    public void StartDialogue()
//    {
//        GameFlowManager.Instance.dialogueManager.StartGuardianDialogue(this);
//    }

//    public void TakeDamage(int damage)
//    {
//        life -= damage;

//        if (life <= 0)
//        {
//            life = 0;
//            Die();
//        }
//    }

//    void Die()
//    {
//        GameFlowManager.Instance.combatManager.EndCombat(true);
//    }
//}
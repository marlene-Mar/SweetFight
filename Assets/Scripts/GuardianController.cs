//using UnityEngine;
//using UnityEngine.AI;
//using System.Collections;

//// ═══════════════════════════════════════════════════════════════════
////  GuardianController  —  versión corregida y optimizada
////
////  FIXES aplicados:
////  [8]  La animación de muerte se completa ANTES de llamar BecomeAlly()
////  [9]  En modo aliado se ignora a otros guardianes por tag "Guardian"
////  [10] DetectPlayer() respeta el flag global IsInCombat del CombatManager
////  [+]  Eliminada la doble llamada a ExitCombat() del jugador
////  [+]  AttackCooldown ya no duplica el reseteo del timer
//// ═══════════════════════════════════════════════════════════════════

//[RequireComponent(typeof(NavMeshAgent))]
//[RequireComponent(typeof(Animator))]
//public class GuardianController : MonoBehaviour
//{
//    // ── Referencias ──────────────────────────────────────────────
//    private NavMeshAgent agent;
//    private Animator animator;
//    private Transform player;
//    private PompompurinController playerController;
//    private DialogueManager dialogueManager;
//    private CombatManager combatManager;

//    // ── Patrulla ─────────────────────────────────────────────────
//    [Header("Patrulla")]
//    public float patrolRadius = 25f;
//    public float waitTimeBetweenPoints = 1.5f;
//    public float detectionDistance = 5f;

//    private float waitTimer;
//    private bool hasDestination;

//    // ── Interacción ───────────────────────────────────────────────
//    [Header("Interacción")]
//    public float interactionCooldown = 15f;
//    private bool canInteract = true;
//    public bool playerNearby;      // acceso desde GuardianSpawner si es necesario

//    // ── Combate ───────────────────────────────────────────────────
//    [Header("Combate")]
//    public float timeBetweenAttacks = 2f;
//    public int maxHealth = 50;
//    public int weaponDamage = 10;

//    [Header("Arma")]
//    public GameObject weaponObject;
//    public Collider weaponCollider;

//    [Header("Timing del arma")]
//    public float weaponActivationDelay = 0.3f;  // segundos hasta activar el collider
//    public float weaponActiveDuration = 0.5f;  // segundos que el collider permanece activo

//    private float attackTimer;
//    private bool isAttacking;
//    private bool canReceiveDamage;
//    private int currentHealth;

//    // ── Aliado ────────────────────────────────────────────────────
//    [Header("Modo Aliado")]
//    public float allyDuration = 60f;
//    public float followDistance = 3f;
//    public float allyDetectionRange = 10f;
//    public LayerMask enemyLayer;

//    private float allyTimer;
//    private bool isAlly;
//    private Transform currentEnemyTarget;

//    // ── Evento de vida (para UI / GameManager) ────────────────────
//    public System.Action<int, int> OnVidaChanged;

//    // ── Estado interno ────────────────────────────────────────────
//    private enum GuardianState
//    {
//        Patrolling,
//        Greeting,
//        Talking,
//        Combat,
//        Ally,
//        AllyDefending
//    }
//    private GuardianState currentState;

//    // ═════════════════════════════════════════════════════════════
//    //  CICLO DE VIDA
//    // ═════════════════════════════════════════════════════════════
//    void Awake()
//    {
//        agent = GetComponent<NavMeshAgent>();
//        animator = GetComponent<Animator>();
//        dialogueManager = FindObjectOfType<DialogueManager>();
//        combatManager = FindObjectOfType<CombatManager>();
//        currentHealth = maxHealth;
//    }

//    void Start()
//    {
//        // Configurar collider del arma
//        if (weaponCollider == null && weaponObject != null)
//            weaponCollider = weaponObject.GetComponent<Collider>();

//        if (weaponCollider != null)
//        {
//            weaponCollider.enabled = false;
//            if (weaponCollider.GetComponent<GuardianWeaponCollider>() == null)
//                weaponCollider.gameObject.AddComponent<GuardianWeaponCollider>();
//        }

//        if (weaponObject != null)
//            weaponObject.SetActive(false);

//        // Suscribirse al GameManager si existe
//        GameManager gm = FindObjectOfType<GameManager>();
//        if (gm != null)
//            OnVidaChanged += gm.UpdateHealthBarGuardian;
//    }

//    // Llamado por GuardianSpawner al instanciar
//    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
//    {
//        player = playerTransform;
//        playerController = player.GetComponent<PompompurinController>();
//        currentState = GuardianState.Patrolling;
//        SetWalk(true);
//        MoveToRandomPoint();
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
//                LookAtPlayer();
//                break;

//            case GuardianState.Combat:
//                LookAtPlayer();
//                CombatBehaviour();
//                break;

//            case GuardianState.Ally:
//                AllyBehaviour();
//                break;

//            case GuardianState.AllyDefending:
//                DefendPlayerBehaviour();
//                break;
//        }
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  PATRULLA
//    // ═════════════════════════════════════════════════════════════
//    public void PatrolBehaviour()
//    {
//        if (!agent.pathPending && agent.remainingDistance < 0.5f)
//        {
//            SetWalk(false);
//            waitTimer += Time.deltaTime;

//            if (waitTimer >= waitTimeBetweenPoints)
//            {
//                MoveToRandomPoint();
//                waitTimer = 0f;
//            }
//        }
//        else
//        {
//            SetWalk(true);
//        }
//    }

//    public void MoveToRandomPoint()
//    {
//        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + transform.position;

//        NavMeshHit hit;
//        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
//        {
//            agent.SetDestination(hit.position);
//            hasDestination = true;
//        }
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  DETECCIÓN DEL JUGADOR
//    //  FIX [10]: ignorar si ya hay un combate activo en la escena
//    // ═════════════════════════════════════════════════════════════
//    void DetectPlayer()
//    {
//        if (!canInteract) return;
//        if (currentState != GuardianState.Patrolling) return;
//        if (combatManager != null && combatManager.IsInCombat) return; 

//        float distance = Vector3.Distance(transform.position, player.position);
//        if (distance <= detectionDistance)
//            StartGreeting();
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  SALUDO → DIÁLOGO → COMBATE
//    // ═════════════════════════════════════════════════════════════
//    void StartGreeting()
//    {
//        canInteract = false;
//        currentState = GuardianState.Greeting;

//        agent.isStopped = true;
//        SetWalk(false);

//        animator.SetBool("isGreeting", true);

//        playerController?.EnterDialogue();
//        Invoke(nameof(StartDialogue), 2f);
//    }

//    void StartDialogue()
//    {
//        animator.SetBool("isGreeting", false);
//        currentState = GuardianState.Talking;
//        dialogueManager?.StartGuardianDialogue(this);
//    }

//    public void EndDialogue()
//    {
//        currentState = GuardianState.Combat;

//        playerController?.ExitDialogue();
//        playerController?.StartCombatAfterDialogue();

//        combatManager?.StartCombat(this, playerController ?? FindObjectOfType<PompompurinController>());

//        if (weaponObject != null) weaponObject.SetActive(true);
//        StartCombat();
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  COMBATE
//    // ═════════════════════════════════════════════════════════════
//    public void StartCombat()
//    {
//        animator.SetBool("InCombat", true);
//        canReceiveDamage = true;
//        attackTimer = 0f;
//        isAttacking = false;

//        // Primer ataque con pequeño delay para que la animación arranque
//        Invoke(nameof(ExecuteAttack), 0.5f);
//    }

//    void CombatBehaviour()
//    {
//        if (isAttacking) return;

//        attackTimer += Time.deltaTime;
//        if (attackTimer >= timeBetweenAttacks)
//        {
//            attackTimer = 0f;   // resetear ANTES de llamar ExecuteAttack
//            ExecuteAttack();
//        }
//    }

//    void ExecuteAttack()
//    {
//        // Solo atacar en estados válidos
//        if (currentState != GuardianState.Combat &&
//            currentState != GuardianState.AllyDefending) return;

//        isAttacking = true;
//        animator.SetTrigger("Attack");
//        Debug.Log($"{name}: ejecutando ataque.");

//        StartCoroutine(ActivateWeaponCollider(weaponActivationDelay, weaponActiveDuration));
//        // FIX: AttackCooldown solo controla isAttacking, el timer ya se reseteó arriba
//        StartCoroutine(AttackCooldown());
//    }

//    IEnumerator ActivateWeaponCollider(float delay, float duration)
//    {
//        yield return new WaitForSeconds(delay);

//        if (weaponCollider != null)
//            weaponCollider.GetComponent<GuardianWeaponCollider>()?.ResetHit();

//        EnableWeaponCollider();
//        yield return new WaitForSeconds(duration);
//        DisableWeaponCollider();
//    }

//    // Solo libera el flag; el timer ya fue reseteado en CombatBehaviour/DefendPlayerBehaviour
//    IEnumerator AttackCooldown()
//    {
//        yield return new WaitForSeconds(timeBetweenAttacks);
//        isAttacking = false;
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  DAÑO Y MUERTE
//    // ═════════════════════════════════════════════════════════════
//    public void TakeDamage(int damage)
//    {
//        if (!canReceiveDamage) return;

//        currentHealth = Mathf.Max(0, currentHealth - damage);
//        Debug.Log($"{name} vida: {currentHealth}/{maxHealth}");
//        OnVidaChanged?.Invoke(currentHealth, maxHealth);

//        if (currentHealth == 0) Die();
//    }

//    void Die()
//    {
//        Debug.Log($"{name} derrotado.");
//        canReceiveDamage = false;
//        isAttacking = false;

//        animator.SetBool("InCombat", false);
//        animator.SetBool("Die", true);

//        // Detener movimiento y parar coroutines de combate
//        agent.isStopped = true;
//        StopAllCoroutines();
//        DisableWeaponCollider();
//        if (weaponObject != null) weaponObject.SetActive(false);

//        // [FIX 8] Avisar al CombatManager de la victoria AHORA,
//        // pero retrasar BecomeAlly hasta que la animación de muerte termine.
//        combatManager?.EndCombat(true);

//        // Esperar la duración de la animación de muerte antes de convertirse en aliado.
//        // Ajusta el delay al largo real del clip "Muerte" en tu Animator.
//        StartCoroutine(BecomeAllyAfterDeathAnimation(2.5f));
//    }

//    // ─────────────────────────────────────────────────────────────
//    //  FIX [8]: espera la animación de muerte y luego se levanta
//    // ─────────────────────────────────────────────────────────────
//    IEnumerator BecomeAllyAfterDeathAnimation(float deathAnimDuration)
//    {
//        yield return new WaitForSeconds(deathAnimDuration);
//        BecomeAlly();
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  MODO ALIADO
//    // ═════════════════════════════════════════════════════════════
//    public void BecomeAlly()
//    {
//        isAlly = true;
//        allyTimer = 0f;
//        currentState = GuardianState.Ally;

//        // Limpiar estado de muerte y combate
//        animator.SetBool("Die", false);
//        animator.SetBool("InCombat", false);
//        SetWalk(false);

//        agent.isStopped = false;
//        canReceiveDamage = false;

//        // Cambiar tag para que otros guardianes no nos detecten como enemigos [FIX 9]
//        gameObject.tag = "GuardianAlly";

//        if (weaponObject != null) weaponObject.SetActive(false);

//        Debug.Log($"{name} ahora es tu aliado por {allyDuration}s.");
//        StartCoroutine(AllyWarningRoutine());
//    }

//    // FIX [9]: Al buscar enemigos se excluyen objetos con tag "Guardian" o "GuardianAlly"
//    void AllyBehaviour()
//    {
//        allyTimer += Time.deltaTime;
//        if (allyTimer >= allyDuration) { EndAllyMode(); return; }

//        // Buscar enemigos cercanos, ignorando guardianes
//        Collider[] hits = Physics.OverlapSphere(transform.position, allyDetectionRange, enemyLayer);
//        Transform nearestEnemy = null;

//        foreach (var hit in hits)
//        {
//            // Ignorar a otros guardianes (propios o enemigos convertidos)
//            if (hit.CompareTag("Guardian") || hit.CompareTag("GuardianAlly")) continue;
//            nearestEnemy = hit.transform;
//            break;
//        }

//        if (nearestEnemy != null)
//        {
//            currentEnemyTarget = nearestEnemy;
//            currentState = GuardianState.AllyDefending;
//            animator.SetBool("InCombat", true);
//            if (weaponObject != null) weaponObject.SetActive(true);
//            Debug.Log($"{name}: detectó enemigo {currentEnemyTarget.name}, entrando en defensa.");
//        }
//        else
//        {
//            FollowPlayer();
//        }
//    }

//    void DefendPlayerBehaviour()
//    {
//        allyTimer += Time.deltaTime;
//        if (allyTimer >= allyDuration) { EndAllyMode(); return; }

//        // Objetivo destruido o salió de rango → volver a seguir al jugador
//        if (currentEnemyTarget == null ||
//            Vector3.Distance(transform.position, currentEnemyTarget.position) > allyDetectionRange * 1.5f)
//        {
//            currentEnemyTarget = null;
//            currentState = GuardianState.Ally;
//            animator.SetBool("InCombat", false);
//            if (weaponObject != null) weaponObject.SetActive(false);
//            return;
//        }

//        // Moverse hacia el enemigo y atacar
//        agent.isStopped = false;
//        agent.SetDestination(currentEnemyTarget.position);
//        LookAtTarget(currentEnemyTarget);

//        if (!isAttacking)
//        {
//            attackTimer += Time.deltaTime;
//            if (attackTimer >= timeBetweenAttacks)
//            {
//                attackTimer = 0f;
//                ExecuteAttack();
//            }
//        }
//    }

//    void FollowPlayer()
//    {
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

//        if (distanceToPlayer > followDistance)
//        {
//            agent.isStopped = false;
//            agent.SetDestination(player.position);
//            SetWalk(true);
//        }
//        else
//        {
//            agent.isStopped = true;
//            SetWalk(false);
//            LookAtPlayer();
//        }
//    }

//    void EndAllyMode()
//    {
//        isAlly = false;
//        currentState = GuardianState.Patrolling;

//        animator.SetBool("InCombat", false);

//        if (weaponObject != null) weaponObject.SetActive(false);

//        agent.isStopped = false;
//        currentHealth = maxHealth;

//        // Restaurar tag original para que otros guardianes puedan interactuar con el jugador
//        gameObject.tag = "Guardian";

//        Debug.Log($"{name} deja de ser aliado y vuelve a patrullar.");
//        StartCoroutine(ResetInteraction());
//        MoveToRandomPoint();
//    }

//    // Avisa 10 s antes de que expire el modo aliado
//    IEnumerator AllyWarningRoutine()
//    {
//        yield return new WaitForSeconds(allyDuration - 10f);
//        Debug.Log($"¡{name} te abandonará en 10 segundos!");
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  FINAL DE COMBATE (el guardián sobrevive → vuelve a patrullar)
//    // ═════════════════════════════════════════════════════════════
//    public void EndCombat()
//    {
//        currentState = GuardianState.Patrolling;
//        isAttacking = false;
//        attackTimer = 0f;
//        canReceiveDamage = false;

//        animator.SetBool("InCombat", false);

//        agent.isStopped = false;

//        if (weaponObject != null) weaponObject.SetActive(false);

//        DisableWeaponCollider();
//        MoveToRandomPoint();
//        StartCoroutine(ResetInteraction());
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  ARMA
//    // ═════════════════════════════════════════════════════════════
//    public void EnableWeaponCollider()
//    {
//        if (weaponCollider != null)
//        {
//            weaponCollider.enabled = true;
//            Debug.Log($"{name}: arma activada.");
//        }
//    }

//    public void DisableWeaponCollider()
//    {
//        if (weaponCollider != null)
//        {
//            weaponCollider.enabled = false;
//        }
//    }

//    /// <summary>Notificación desde GuardianWeaponCollider.</summary>
//    public void NotifyWeaponHit()
//    {
//        if (isAlly)
//            Debug.Log($"{name} (aliado) golpeó a un enemigo.");
//        else
//            combatManager?.OnGuardianHit(weaponDamage);
//    }

//    // ═════════════════════════════════════════════════════════════
//    //  UTILIDADES
//    // ═════════════════════════════════════════════════════════════
//    void LookAtPlayer() => LookAtTarget(player);

//    void LookAtTarget(Transform target)
//    {
//        if (target == null) return;
//        Vector3 dir = target.position - transform.position;
//        dir.y = 0;
//        if (dir != Vector3.zero)
//            transform.rotation = Quaternion.Slerp(transform.rotation,
//                                                   Quaternion.LookRotation(dir),
//                                                   Time.deltaTime * 5f);
//    }

//    void SetWalk(bool value) => animator.SetBool("Walk", value);

//    IEnumerator ResetInteraction()
//    {
//        yield return new WaitForSeconds(interactionCooldown);
//        canInteract = true;
//    }

//    // ── Getters públicos ──────────────────────────────────────────
//    public int GetCurrentHealth() => currentHealth;
//    public bool IsAlly() => isAlly;
//    public bool CanReceiveDamage() => canReceiveDamage;
//}

//// ═══════════════════════════════════════════════════════════════════
////  GuardianWeaponCollider  —  sin cambios de lógica, solo limpieza
//// ═══════════════════════════════════════════════════════════════════
//public class GuardianWeaponCollider : MonoBehaviour
//{
//    [SerializeField] private int damage = 10;
//    private bool hasHit;

//    private GuardianController guardianController;
//    private CombatManager combatManager;

//    void Start()
//    {
//        guardianController = GetComponentInParent<GuardianController>();
//        combatManager = FindObjectOfType<CombatManager>();

//        if (guardianController == null)
//            Debug.LogError($"[GuardianWeaponCollider] {gameObject.name}: GuardianController no encontrado en el padre.");
//        if (combatManager == null)
//            Debug.LogError("[GuardianWeaponCollider] CombatManager no encontrado en la escena.");
//    }

//    void OnEnable() => hasHit = false;

//    public void ResetHit() => hasHit = false;

//    void OnTriggerEnter(Collider other)
//    {
//        if (guardianController == null || hasHit) return;

//        if (!guardianController.IsAlly())
//        {
//            // Modo enemigo: golpear al jugador
//            if (other.CompareTag("Player"))
//            {
//                Debug.Log($"Arma del guardián golpeó a {other.name}.");
//                combatManager?.OnGuardianHit(damage);
//                guardianController.NotifyWeaponHit();
//                hasHit = true;
//            }
//        }
//        else
//        {
//            // Modo aliado: golpear solo a enemigos (nunca a otros guardianes) [FIX 9]
//            if (other.CompareTag("Enemy") &&
//                !other.CompareTag("Guardian") &&
//                !other.CompareTag("GuardianAlly"))
//            {
//                Debug.Log($"Guardián aliado golpeó a {other.name}.");
//                guardianController.NotifyWeaponHit();
//                hasHit = true;
//            }
//        }
//    }
//}

using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// ═══════════════════════════════════════════════════════════════════
//  GuardianController  —  versión corregida y optimizada
//
//  FIXES aplicados:
//  [8]  La animación de muerte se completa ANTES de llamar BecomeAlly()
//  [9]  En modo aliado se ignora a otros guardianes por tag "Guardian"
//  [10] DetectPlayer() respeta el flag global IsInCombat del CombatManager
//  [+]  Eliminada la doble llamada a ExitCombat() del jugador
//  [+]  AttackCooldown ya no duplica el reseteo del timer
// ═══════════════════════════════════════════════════════════════════

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class GuardianController : MonoBehaviour
{
    // ── Referencias ──────────────────────────────────────────────
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private PompompurinController playerController;
    private DialogueManager dialogueManager;
    private CombatManager combatManager;

    // ── Patrulla ─────────────────────────────────────────────────
    [Header("Patrulla")]
    public float patrolRadius = 25f;
    public float waitTimeBetweenPoints = 1.5f;
    public float detectionDistance = 5f;

    private float waitTimer;
    private bool hasDestination;

    // ── Interacción ───────────────────────────────────────────────
    [Header("Interacción")]
    public float interactionCooldown = 15f;
    private bool canInteract = true;
    public bool playerNearby;      // acceso desde GuardianSpawner si es necesario

    // ── Combate ───────────────────────────────────────────────────
    [Header("Combate")]
    public float timeBetweenAttacks = 2f;
    public int maxHealth = 50;
    public int weaponDamage = 10;

    [Header("Arma")]
    public GameObject weaponObject;
    public Collider weaponCollider;

    [Header("Timing del arma")]
    public float weaponActivationDelay = 0.3f;  // segundos hasta activar el collider
    public float weaponActiveDuration = 0.5f;  // segundos que el collider permanece activo

    private float attackTimer;
    private bool isAttacking;
    private bool canReceiveDamage;
    private int currentHealth;

    // ── Aliado ────────────────────────────────────────────────────
    [Header("Modo Aliado")]
    public float allyDuration = 60f;
    public float followDistance = 3f;
    public float allyDetectionRange = 10f;
    public LayerMask enemyLayer;

    private float allyTimer;
    private bool isAlly;
    private Transform currentEnemyTarget;

    // ── Evento de vida (para UI / GameManager) ────────────────────
    public System.Action<int, int> OnVidaChanged;

    // ── Evento para contador de aliados ────────────────────────────
    public static System.Action OnGuardianBecameAlly;
    public static System.Action OnGuardianLeftAlly;

    // ── Estado interno ────────────────────────────────────────────
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

    // ═════════════════════════════════════════════════════════════
    //  CICLO DE VIDA
    // ═════════════════════════════════════════════════════════════
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
        // Configurar collider del arma
        if (weaponCollider == null && weaponObject != null)
            weaponCollider = weaponObject.GetComponent<Collider>();

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            if (weaponCollider.GetComponent<GuardianWeaponCollider>() == null)
                weaponCollider.gameObject.AddComponent<GuardianWeaponCollider>();
        }

        if (weaponObject != null)
            weaponObject.SetActive(false);

        // Suscribirse al GameManager si existe
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
            OnVidaChanged += gm.UpdateHealthBarGuardian;
    }

    // Llamado por GuardianSpawner al instanciar
    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
    {
        player = playerTransform;
        playerController = player.GetComponent<PompompurinController>();
        currentState = GuardianState.Patrolling;
        SetWalk(true);
        MoveToRandomPoint();
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

    // ═════════════════════════════════════════════════════════════
    //  PATRULLA
    // ═════════════════════════════════════════════════════════════
    public void PatrolBehaviour()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetWalk(false);
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeBetweenPoints)
            {
                MoveToRandomPoint();
                waitTimer = 0f;
            }
        }
        else
        {
            SetWalk(true);
        }
    }

    public void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            hasDestination = true;
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  DETECCIÓN DEL JUGADOR
    //  FIX [10]: ignorar si ya hay un combate activo en la escena
    // ═════════════════════════════════════════════════════════════
    void DetectPlayer()
    {
        if (!canInteract) return;
        if (currentState != GuardianState.Patrolling) return;
        if (combatManager != null && combatManager.IsInCombat) return; // [10]

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionDistance)
            StartGreeting();
    }

    // ═════════════════════════════════════════════════════════════
    //  SALUDO → DIÁLOGO → COMBATE
    // ═════════════════════════════════════════════════════════════
    void StartGreeting()
    {
        canInteract = false;
        currentState = GuardianState.Greeting;

        agent.isStopped = true;
        SetWalk(false);

        animator.SetBool("isGreeting", true);

        playerController?.EnterDialogue();
        Invoke(nameof(StartDialogue), 2f);
    }

    void StartDialogue()
    {
        animator.SetBool("isGreeting", false);
        currentState = GuardianState.Talking;
        dialogueManager?.StartGuardianDialogue(this);
    }

    public void EndDialogue()
    {
        currentState = GuardianState.Combat;

        playerController?.ExitDialogue();
        playerController?.StartCombatAfterDialogue();

        combatManager?.StartCombat(this, playerController ?? FindObjectOfType<PompompurinController>());

        if (weaponObject != null) weaponObject.SetActive(true);
        StartCombat();
    }

    // ═════════════════════════════════════════════════════════════
    //  COMBATE
    // ═════════════════════════════════════════════════════════════
    public void StartCombat()
    {
        animator.SetBool("InCombat", true);
        canReceiveDamage = true;
        attackTimer = 0f;
        isAttacking = false;

        // Primer ataque con pequeño delay para que la animación arranque
        Invoke(nameof(ExecuteAttack), 0.5f);
    }

    void CombatBehaviour()
    {
        if (isAttacking) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= timeBetweenAttacks)
        {
            attackTimer = 0f;   // resetear ANTES de llamar ExecuteAttack
            ExecuteAttack();
        }
    }

    void ExecuteAttack()
    {
        // Solo atacar en estados válidos
        if (currentState != GuardianState.Combat &&
            currentState != GuardianState.AllyDefending) return;

        isAttacking = true;
        animator.SetTrigger("Attack");
        Debug.Log($"{name}: ejecutando ataque.");

        StartCoroutine(ActivateWeaponCollider(weaponActivationDelay, weaponActiveDuration));
        // FIX: AttackCooldown solo controla isAttacking, el timer ya se reseteó arriba
        StartCoroutine(AttackCooldown());
    }

    IEnumerator ActivateWeaponCollider(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        if (weaponCollider != null)
            weaponCollider.GetComponent<GuardianWeaponCollider>()?.ResetHit();

        EnableWeaponCollider();
        yield return new WaitForSeconds(duration);
        DisableWeaponCollider();
    }

    // Solo libera el flag; el timer ya fue reseteado en CombatBehaviour/DefendPlayerBehaviour
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        isAttacking = false;
    }

    // ═════════════════════════════════════════════════════════════
    //  DAÑO Y MUERTE
    // ═════════════════════════════════════════════════════════════
    public void TakeDamage(int damage)
    {
        if (!canReceiveDamage) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{name} vida: {currentHealth}/{maxHealth}");
        OnVidaChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0) Die();
    }

    void Die()
    {
        Debug.Log($"{name} derrotado.");
        canReceiveDamage = false;
        isAttacking = false;

        animator.SetBool("InCombat", false);
        animator.SetBool("Die", true);

        // Detener movimiento y parar coroutines de combate
        agent.isStopped = true;
        StopAllCoroutines();
        DisableWeaponCollider();
        if (weaponObject != null) weaponObject.SetActive(false);

        // [FIX 8] Avisar al CombatManager de la victoria AHORA,
        // pero retrasar BecomeAlly hasta que la animación de muerte termine.
        combatManager?.EndCombat(true);

        // Esperar la duración de la animación de muerte antes de convertirse en aliado.
        // Ajusta el delay al largo real del clip "Muerte" en tu Animator.
        StartCoroutine(BecomeAllyAfterDeathAnimation(2.5f));
    }

    // ─────────────────────────────────────────────────────────────
    //  FIX [8]: espera la animación de muerte y luego se levanta
    // ─────────────────────────────────────────────────────────────
    IEnumerator BecomeAllyAfterDeathAnimation(float deathAnimDuration)
    {
        yield return new WaitForSeconds(deathAnimDuration);
        BecomeAlly();
    }

    // ═════════════════════════════════════════════════════════════
    //  MODO ALIADO
    // ═════════════════════════════════════════════════════════════
    public void BecomeAlly()
    {
        isAlly = true;
        allyTimer = 0f;
        currentState = GuardianState.Ally;

        // Limpiar estado de muerte y combate
        animator.SetBool("Die", false);
        animator.SetBool("InCombat", false);
        SetWalk(false);

        agent.isStopped = false;
        canReceiveDamage = false;
        attackTimer = 0f;
        isAttacking = false;

        // Cambiar tag para que otros guardianes no nos detecten como enemigos [FIX 9]
        gameObject.tag = "GuardianAlly";

        if (weaponObject != null) weaponObject.SetActive(false);

        Debug.Log($"{name} ahora es tu aliado por {allyDuration}s.");

        // Notificar que se agregó un aliado (para contador)
        OnGuardianBecameAlly?.Invoke();

        StartCoroutine(AllyWarningRoutine());
    }

    // FIX [9]: Al buscar enemigos se excluyen objetos con tag "Guardian" o "GuardianAlly"
    void AllyBehaviour()
    {
        allyTimer += Time.deltaTime;
        if (allyTimer >= allyDuration) { EndAllyMode(); return; }

        // Buscar enemigos cercanos, ignorando guardianes
        Collider[] hits = Physics.OverlapSphere(transform.position, allyDetectionRange, enemyLayer);
        Transform nearestEnemy = null;

        foreach (var hit in hits)
        {
            // Ignorar a otros guardianes (propios o enemigos convertidos)
            if (hit.CompareTag("Guardian") || hit.CompareTag("GuardianAlly")) continue;
            nearestEnemy = hit.transform;
            break;
        }

        if (nearestEnemy != null)
        {
            currentEnemyTarget = nearestEnemy;
            currentState = GuardianState.AllyDefending;
            animator.SetBool("InCombat", true);
            if (weaponObject != null) weaponObject.SetActive(true);
            Debug.Log($"{name}: detectó enemigo {currentEnemyTarget.name}, entrando en defensa.");
        }
        else
        {
            FollowPlayer();
        }
    }

    void DefendPlayerBehaviour()
    {
        allyTimer += Time.deltaTime;
        if (allyTimer >= allyDuration) { EndAllyMode(); return; }

        // Objetivo destruido o salió de rango → volver a seguir al jugador
        if (currentEnemyTarget == null ||
            Vector3.Distance(transform.position, currentEnemyTarget.position) > allyDetectionRange * 1.5f)
        {
            currentEnemyTarget = null;
            currentState = GuardianState.Ally;
            animator.SetBool("InCombat", false);
            if (weaponObject != null) weaponObject.SetActive(false);
            attackTimer = 0f;
            isAttacking = false;
            agent.isStopped = false;  // Importante: reactivar el movimiento
            return;
        }

        // Moverse hacia el enemigo solo si está lejos
        float distanceToEnemy = Vector3.Distance(transform.position, currentEnemyTarget.position);

        if (distanceToEnemy > 2.5f) // Rango de ataque
        {
            agent.isStopped = false;
            agent.SetDestination(currentEnemyTarget.position);
            SetWalk(true);
        }
        else
        {
            agent.isStopped = true;
            SetWalk(false);
        }

        LookAtTarget(currentEnemyTarget);

        // Atacar solo si está en rango
        if (distanceToEnemy <= 2.5f && !isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= timeBetweenAttacks)
            {
                attackTimer = 0f;
                ExecuteAttack();
            }
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

    void EndAllyMode()
    {
        isAlly = false;
        currentState = GuardianState.Patrolling;

        animator.SetBool("InCombat", false);

        if (weaponObject != null) weaponObject.SetActive(false);

        agent.isStopped = false;
        currentHealth = maxHealth;
        attackTimer = 0f;
        isAttacking = false;

        // Restaurar tag original para que otros guardianes puedan interactuar con el jugador
        gameObject.tag = "Guardian";

        Debug.Log($"{name} deja de ser aliado y vuelve a patrullar.");

        // Notificar que se perdió un aliado (para contador)
        OnGuardianLeftAlly?.Invoke();

        StartCoroutine(ResetInteraction());
        MoveToRandomPoint();
    }

    // Avisa 10 s antes de que expire el modo aliado
    IEnumerator AllyWarningRoutine()
    {
        yield return new WaitForSeconds(allyDuration - 10f);
        Debug.Log($"¡{name} te abandonará en 10 segundos!");
    }

    // ═════════════════════════════════════════════════════════════
    //  FINAL DE COMBATE (el guardián sobrevive → vuelve a patrullar)
    // ═════════════════════════════════════════════════════════════
    public void EndCombat()
    {
        currentState = GuardianState.Patrolling;
        isAttacking = false;
        attackTimer = 0f;
        canReceiveDamage = false;

        animator.SetBool("InCombat", false);

        agent.isStopped = false;

        if (weaponObject != null) weaponObject.SetActive(false);

        DisableWeaponCollider();
        MoveToRandomPoint();
        StartCoroutine(ResetInteraction());
    }

    // ═════════════════════════════════════════════════════════════
    //  ARMA
    // ═════════════════════════════════════════════════════════════
    public void EnableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            Debug.Log($"{name}: arma activada.");
        }
    }

    public void DisableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    /// <summary>Notificación desde GuardianWeaponCollider.</summary>
    public void NotifyWeaponHit()
    {
        if (isAlly)
            Debug.Log($"{name} (aliado) golpeó a un enemigo.");
        else
            combatManager?.OnGuardianHit(weaponDamage);
    }

    // ═════════════════════════════════════════════════════════════
    //  UTILIDADES
    // ═════════════════════════════════════════════════════════════
    void LookAtPlayer() => LookAtTarget(player);

    void LookAtTarget(Transform target)
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                   Quaternion.LookRotation(dir),
                                                   Time.deltaTime * 5f);
    }

    void SetWalk(bool value) => animator.SetBool("Walk", value);

    IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    // ── Getters públicos ──────────────────────────────────────────
    public int GetCurrentHealth() => currentHealth;
    public bool IsAlly() => isAlly;
    public bool CanReceiveDamage() => canReceiveDamage;
}

// ═══════════════════════════════════════════════════════════════════
//  GuardianWeaponCollider  —  sin cambios de lógica, solo limpieza
// ═══════════════════════════════════════════════════════════════════
public class GuardianWeaponCollider : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    private bool hasHit;

    private GuardianController guardianController;
    private CombatManager combatManager;

    void Start()
    {
        guardianController = GetComponentInParent<GuardianController>();
        combatManager = FindObjectOfType<CombatManager>();

        if (guardianController == null)
            Debug.LogError($"[GuardianWeaponCollider] {gameObject.name}: GuardianController no encontrado en el padre.");
        if (combatManager == null)
            Debug.LogError("[GuardianWeaponCollider] CombatManager no encontrado en la escena.");
    }

    void OnEnable() => hasHit = false;

    public void ResetHit() => hasHit = false;

    void OnTriggerEnter(Collider other)
    {
        if (guardianController == null || hasHit) return;

        if (!guardianController.IsAlly())
        {
            // Modo enemigo: golpear al jugador
            if (other.CompareTag("Player"))
            {
                Debug.Log($"Arma del guardián golpeó a {other.name}.");
                combatManager?.OnGuardianHit(damage);
                guardianController.NotifyWeaponHit();
                hasHit = true;
            }
        }
        else
        {
            // Modo aliado: golpear solo a enemigos (nunca a otros guardianes) [FIX 9]
            if (other.CompareTag("Enemy") &&
                !other.CompareTag("Guardian") &&
                !other.CompareTag("GuardianAlly"))
            {
                Debug.Log($"Guardián aliado golpeó a {other.name}.");
                guardianController.NotifyWeaponHit();
                hasHit = true;
            }
        }
    }
}
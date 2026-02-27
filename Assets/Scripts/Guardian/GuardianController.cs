using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Controla el comportamiento del Guardián: Patrulla, Diálogo, Combate y Modo Aliado.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class GuardianController : MonoBehaviour
{
    // ═════════════════════════════════════════════════════════════
    //  VARIABLES Y REFERENCIAS
    // ═════════════════════════════════════════════════════════════

    #region Referencias de Componentes
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private PompompurinController playerController;
    private DialogueManager dialogueManager;
    private CombatManager combatManager;
    #endregion

    #region Configuración de Diálogo
    [Header("Diálogo")]
    public Dialogos guardianDialogue;
    #endregion

    #region Configuración de Patrulla
    [Header("Patrulla")]
    public float patrolRadius = 25f;
    public float waitTimeBetweenPoints = 1.5f;
    public float detectionDistance = 5f;

    private float waitTimer;
    private bool hasDestination;
    #endregion

    #region Configuración de Interacción
    [Header("Interacción")]
    public float interactionCooldown = 30f;
    private bool canInteract = true;
    public bool playerNearby;
    #endregion

    #region Configuración de Combate
    [Header("Combate")]
    public float timeBetweenAttacks = 1.5f;
    public int maxHealth = 50;
    public int weaponDamage = 10;

    [Header("Arma")]
    public GameObject weaponObject;
    public Collider weaponCollider;

    [Header("Timing del arma")]
    public float weaponActivationDelay = 0.3f;
    public float weaponActiveDuration = 0.5f;

    private float attackTimer;
    private bool isAttacking;
    private bool canReceiveDamage;
    private int currentHealth;
    #endregion

    #region Configuración Modo Aliado
    [Header("Modo Aliado")]
    public float allyDuration = 60f;
    public float followDistance = 5f;
    public float allyDetectionRange = 10f;
    public LayerMask enemyLayer;

    private float allyTimer;
    private bool isAlly;
    private Transform currentEnemyTarget;
    private CamemiController camemiTarget;
    private bool playerInDialogueWithCamemi = false;
    #endregion

    #region Eventos / Actions
    public System.Action<int, int> OnVidaChanged;
    public static System.Action OnGuardianBecameAlly;
    public static System.Action OnGuardianLeftAlly;
    public static System.Action<float, float> OnAllyTimerUpdated;
    public static System.Action OnAllyTimerEnded;
    #endregion

    #region Máquina de Estados
    private enum GuardianState
    {
        Patrolling,
        Greeting,
        Talking,
        Combat,
        Ally,
        AllyDefending,
        Dead
    }
    private GuardianState currentState;
    #endregion

    // ═════════════════════════════════════════════════════════════
    //  INICIALIZACIÓN (AWAKE / START)
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
        // Configuración inicial del arma y su collider
        if (weaponCollider == null && weaponObject != null)
            weaponCollider = weaponObject.GetComponent<Collider>();

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            if (weaponCollider.GetComponent<GuardianWeaponCollider>() == null)
                weaponCollider.gameObject.AddComponent<GuardianWeaponCollider>();

            var gwc = weaponCollider.GetComponent<GuardianWeaponCollider>();
            if (gwc != null) gwc.SetDamage(weaponDamage);
        }

        if (weaponObject != null)
            weaponObject.SetActive(false);

        // Registro en el UI a través del GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
            OnVidaChanged += gm.UpdateHealthBarGuardian;
    }

    // Inicialización externa (GuardianSpawner)
    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
    {
        player = playerTransform;
        playerController = player.GetComponent<PompompurinController>();
        currentState = GuardianState.Patrolling;
        SetWalk(true);
        MoveToRandomPoint();
    }

    // ═════════════════════════════════════════════════════════════
    //  BUCLE PRINCIPAL (UPDATE / LATEUPDATE)
    // ═════════════════════════════════════════════════════════════

    void Update()
    {
        if (player == null) return;

        // Gestión de la máquina de estados
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

    void LateUpdate()
    {
        // Debug de animaciones críticas
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Die")) Debug.Log("Animación actual: DIE");
        if (stateInfo.IsName("Attack")) Debug.Log("Animación actual: ATTACK");
        if (stateInfo.IsName("Walk")) Debug.Log("Animación actual: WALK");
    }

    // ═════════════════════════════════════════════════════════════
    //  SISTEMA DE PATRULLA Y DETECCIÓN
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

    void DetectPlayer()
    {
        if (!canInteract) return;
        if (currentState != GuardianState.Patrolling) return;
        if (combatManager != null && combatManager.IsInCombat) return;

        // Solo interactúa si el jugador no tiene ya un aliado
        if (GameManager.Instance != null && GameManager.Instance.GetGuardianAllyCount() > 0) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionDistance)
            StartGreeting();
    }

    // ═════════════════════════════════════════════════════════════
    //  SISTEMA DE DIÁLOGOS
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

        if (guardianDialogue != null)
            dialogueManager.StartGuardianDialogue(guardianDialogue, this);
    }

    public void EndDialogue()
    {
        currentState = GuardianState.Combat;
        playerController?.ExitDialogue();
        playerController?.StartCombatAfterDialogue();

        combatManager?.StartGuardianCombat(this, playerController ?? FindObjectOfType<PompompurinController>());

        if (weaponObject != null) weaponObject.SetActive(true);
        StartCombat();
    }

    // ═════════════════════════════════════════════════════════════
    //  SISTEMA DE COMBATE (ENEMIGO)
    // ═════════════════════════════════════════════════════════════

    public void StartCombat()
    {
        Debug.Log($"{name} StartCombat() — canReceiveDamage será true");
        animator.SetBool("InCombat", true);
        canReceiveDamage = true;
        attackTimer = 0f;
        isAttacking = false;

        OnVidaChanged?.Invoke(currentHealth, maxHealth);
        Invoke(nameof(ExecuteAttack), 0.5f);
    }

    void CombatBehaviour()
    {
        if (isAttacking) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= timeBetweenAttacks)
        {
            attackTimer = 0f;
            ExecuteAttack();
        }
    }

    void ExecuteAttack()
    {
        if (currentState != GuardianState.Combat && currentState != GuardianState.AllyDefending) return;

        isAttacking = true;
        animator.SetTrigger("Attack");
        Debug.Log($"{name}: ejecutando ataque.");

        StartCoroutine(ActivateWeaponCollider(weaponActivationDelay, weaponActiveDuration));
        StartCoroutine(AttackCooldown());
    }

    IEnumerator ActivateWeaponCollider(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        if (weaponCollider != null)
            weaponCollider.GetComponent<GuardianWeaponCollider>()?.ResetHit();

        EnableWeaponCollider();

        // Lógica especial de ataque en área si es aliado
        if (isAlly)
        {
            Vector3 puntoDeAtaque = transform.position + transform.forward * 1.2f;
            Collider[] objetosGolpeados = Physics.OverlapSphere(puntoDeAtaque, 2f);

            foreach (var hit in objetosGolpeados)
            {
                CamemiController camemi = hit.GetComponent<CamemiController>() ?? hit.GetComponentInParent<CamemiController>();
                if (camemi != null)
                {
                    camemi.TakeDamageFromGuardian(weaponDamage);
                    Debug.Log($"<color=green>¡GOLPE DEFINITIVO!</color> El Guardián le bajó {weaponDamage} a Camemi.");
                    break;
                }
            }
        }

        yield return new WaitForSeconds(duration);
        DisableWeaponCollider();
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        isAttacking = false;
    }

    // ═════════════════════════════════════════════════════════════
    //  DAÑO Y SALUD
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
        currentState = GuardianState.Dead;

        animator.SetBool("InCombat", false);
        animator.SetBool("Die", true);

        agent.isStopped = true;
        StopAllCoroutines();
        DisableWeaponCollider();
        if (weaponObject != null) weaponObject.SetActive(false);

        combatManager?.EndCombat(true);
        StartCoroutine(BecomeAllyAfterDeathAnimation(0f));
    }

    IEnumerator BecomeAllyAfterDeathAnimation(float deathAnimDuration)
    {
        yield return null;
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return !stateInfo.IsName("Die");
        });

        yield return new WaitForSeconds(0.3f);
        BecomeAlly();
    }

    // ═════════════════════════════════════════════════════════════
    //  MODO ALIADO (Lógica y Comportamiento)
    // ═════════════════════════════════════════════════════════════

    public void BecomeAlly()
    {
        isAlly = true;
        allyTimer = 0f;
        currentState = GuardianState.Ally;

        animator.SetBool("Die", false);
        animator.SetBool("InCombat", false);
        SetWalk(false);

        agent.isStopped = false;
        canReceiveDamage = false;
        attackTimer = 0f;
        isAttacking = false;
        currentHealth = maxHealth;

        gameObject.tag = "GuardianAlly";
        gameObject.layer = LayerMask.NameToLayer("GuardianAlly");

        if (weaponObject != null) weaponObject.SetActive(false);

        Debug.Log($"{name} ahora es tu aliado por {allyDuration}s.");
        OnGuardianBecameAlly?.Invoke();
        StartCoroutine(AllyWarningRoutine());
    }

    void AllyBehaviour()
    {
        TickAllyTimer();
        if (allyTimer >= allyDuration) { EndAllyMode(); return; }

        // Escaneo de enemigos cercanos
        Collider[] hits = Physics.OverlapSphere(transform.position, allyDetectionRange, enemyLayer);
        Transform nearestEnemy = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Guardian") || hit.CompareTag("GuardianAlly")) continue;

            CamemiController camemiCheck = hit.GetComponent<CamemiController>() ?? hit.GetComponentInParent<CamemiController>();
            if (camemiCheck != null)
            {
                if (camemiCheck.VidaActual <= 0 || (CombatManager.Instance != null && !CombatManager.Instance.IsInCombat))
                    continue;
            }

            nearestEnemy = hit.transform;
            break;
        }

        if (nearestEnemy != null)
        {
            currentEnemyTarget = nearestEnemy;
            currentState = GuardianState.AllyDefending;
            animator.SetBool("InCombat", true);
            if (weaponObject != null) weaponObject.SetActive(true);
        }
        else
        {
            FollowPlayer();
        }
    }

    void DefendPlayerBehaviour()
    {
        TickAllyTimer();
        if (allyTimer >= allyDuration) { EndAllyMode(); return; }

        bool shouldDropTarget = false;

        // Validación del objetivo
        if (currentEnemyTarget == null || Vector3.Distance(transform.position, currentEnemyTarget.position) > allyDetectionRange * 1.5f)
        {
            shouldDropTarget = true;
        }
        else
        {
            CamemiController camemiCheck = currentEnemyTarget.GetComponent<CamemiController>() ?? currentEnemyTarget.GetComponentInParent<CamemiController>();
            if (camemiCheck != null)
            {
                if (camemiCheck.VidaActual <= 0 || (CombatManager.Instance != null && !CombatManager.Instance.IsInCombat))
                    shouldDropTarget = true;
            }
        }

        if (shouldDropTarget)
        {
            ReturnToAllyIdle();
            return;
        }

        float distanceToEnemy = Vector3.Distance(transform.position, currentEnemyTarget.position);

        // Movimiento hacia el enemigo
        if (distanceToEnemy > 1.5f)
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

        if (distanceToEnemy <= 1.5f && !isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= timeBetweenAttacks)
            {
                attackTimer = 0f;
                ExecuteAttack();
            }
        }
    }

    private void ReturnToAllyIdle()
    {
        currentEnemyTarget = null;
        currentState = GuardianState.Ally;
        animator.SetBool("InCombat", false);
        if (weaponObject != null) weaponObject.SetActive(false);
        attackTimer = 0f;
        isAttacking = false;
        agent.isStopped = false;
        SetWalk(false);
    }

    void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > followDistance)
        {
            agent.isStopped = false;
            Vector3 followTarget = player.position - player.forward * (followDistance * 0.9f);
            agent.SetDestination(followTarget);
            SetWalk(!agent.pathPending && agent.remainingDistance > 0.5f);
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

        gameObject.tag = "Guardian";
        gameObject.layer = LayerMask.NameToLayer("Guardian");

        Debug.Log($"{name} deja de ser aliado y vuelve a patrullar.");
        OnGuardianLeftAlly?.Invoke();

        StartCoroutine(ResetInteraction());
        MoveToRandomPoint();
        OnAllyTimerEnded?.Invoke();
    }

    void TickAllyTimer()
    {
        allyTimer += Time.deltaTime;
        float remaining = Mathf.Max(0f, allyDuration - allyTimer);
        OnAllyTimerUpdated?.Invoke(remaining, allyDuration);
    }

    IEnumerator AllyWarningRoutine()
    {
        yield return new WaitForSeconds(allyDuration - 10f);
        Debug.Log($"¡{name} te abandonará en 10 segundos!");
    }

    // ═════════════════════════════════════════════════════════════
    //  GESTIÓN DEL ARMA Y COLISIONES
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
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void NotifyWeaponHit()
    {
        if (isAlly)
            Debug.Log($"{name} (aliado) golpeó a un enemigo.");
        else
            combatManager?.OnGuardianHit(weaponDamage);
    }

    // ═════════════════════════════════════════════════════════════
    //  UTILIDADES Y CONTROL DE ANIMACIÓN
    // ═════════════════════════════════════════════════════════════

    void LookAtPlayer() => LookAtTarget(player);

    void LookAtTarget(Transform target)
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    void SetWalk(bool value) => animator.SetBool("Walk", value);

    IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    public void EndCombat()
    {
        CancelInvoke(nameof(ExecuteAttack));
        StopAllCoroutines();

        currentState = GuardianState.Patrolling;
        isAttacking = false;
        attackTimer = 0f;
        canReceiveDamage = false;

        animator.SetBool("InCombat", false);
        animator.SetBool("Walk", false);

        agent.isStopped = false;
        agent.ResetPath();
        currentHealth = maxHealth;
        OnVidaChanged?.Invoke(currentHealth, maxHealth);

        if (weaponObject != null) weaponObject.SetActive(false);
        DisableWeaponCollider();

        MoveToRandomPoint();
        StartCoroutine(ResetInteraction());
    }

    // ═════════════════════════════════════════════════════════════
    //  INTERACCIÓN CON OTROS SCRIPTS (CAMEMI)
    // ═════════════════════════════════════════════════════════════

    public void OnPlayerEnterCamemiDialogue()
    {
        if (!isAlly) return;
        playerInDialogueWithCamemi = true;
        currentEnemyTarget = null;
        currentState = GuardianState.Ally;
        agent.isStopped = true;
        SetWalk(false);
        animator.SetBool("InCombat", false);
        if (weaponObject != null) weaponObject.SetActive(false);
    }

    public void OnPlayerExitCamemiDialogue()
    {
        if (!isAlly) return;
        playerInDialogueWithCamemi = false;
        if (camemiTarget != null && camemiTarget.gameObject.activeInHierarchy)
        {
            currentEnemyTarget = camemiTarget.transform;
            currentState = GuardianState.AllyDefending;
            animator.SetBool("InCombat", true);
            if (weaponObject != null) weaponObject.SetActive(true);
            agent.isStopped = false;
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  PERSISTENCIA (SAVE / LOAD)
    // ═════════════════════════════════════════════════════════════

    public Data.GuardianSaveData GetSaveData()
    {
        return new Data.GuardianSaveData
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            isAlly = isAlly,
            allyTimeRemaining = isAlly ? Mathf.Max(0f, allyDuration - allyTimer) : 0f,
            onCooldown = !canInteract
        };
    }

    public void LoadSaveData(Data.GuardianSaveData data)
    {
        NavMeshHit hit;
        Vector3 savedPos = new Vector3(data.posX, data.posY, data.posZ);
        if (NavMesh.SamplePosition(savedPos, out hit, 5f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        if (data.isAlly)
        {
            BecomeAlly();
            allyTimer = allyDuration - data.allyTimeRemaining;
        }
        else if (data.onCooldown)
        {
            canInteract = false;
            StartCoroutine(ResetInteraction());
        }
    }

    // Getters
    public int GetCurrentHealth() => currentHealth;
    public bool IsAlly() => isAlly;
    public bool CanReceiveDamage() => canReceiveDamage;
}

// ═══════════════════════════════════════════════════════════════════
//  CLASE AUXILIAR: GuardianWeaponCollider
// ═══════════════════════════════════════════════════════════════════

public class GuardianWeaponCollider : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    private bool hasHit;
    private GuardianController guardianController;
    private CombatManager combatManager;

    public void SetDamage(int value) => damage = value;

    void Start()
    {
        guardianController = GetComponentInParent<GuardianController>();
        combatManager = FindObjectOfType<CombatManager>();
    }

    void OnEnable() => hasHit = false;

    public void ResetHit() => hasHit = false;

    void OnTriggerEnter(Collider other)
    {
        if (guardianController == null || hasHit) return;

        if (!guardianController.IsAlly())
        {
            // Daño al Jugador
            if (other.CompareTag("Player"))
            {
                combatManager?.OnGuardianHit(damage);
                guardianController.NotifyWeaponHit();
                hasHit = true;
            }
        }
        else
        {
            // Daño a Camemi u otros enemigos
            CamemiController camemi = other.GetComponent<CamemiController>()
                                   ?? other.GetComponentInParent<CamemiController>()
                                   ?? other.GetComponentInChildren<CamemiController>();

            if (camemi != null)
            {
                hasHit = true;
                guardianController.NotifyWeaponHit();
                camemi.TakeDamageFromGuardian(damage);
            }
        }
    }
}
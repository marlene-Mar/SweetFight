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

    // Patrulla
    public float patrolRadius = 25f;
    public float waitTimeBetweenPoints = 1.5f;
    public float detectionDistance = 5f;

    // Configuración de combate
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private bool canReceiveDamage = false;

    // Salud del guardián
    public int maxHealth = 50;
    private int currentHealth;

    public GameObject weaponObject;
    public Collider weaponCollider;
    public int weaponDamage = 10;

    // Configuración de aliado
    public float allyDuration = 60f;
    public float followDistance = 3f;
    public float allyDetectionRange = 10f;
    public LayerMask enemyLayer;
    private float allyTimer = 0f;
    private bool isAlly = false;

    private float waitTimer;
    private bool hasDestination;
    private bool canInteract = true;
    public float interactionCooldown = 15f;

    public bool playerNearby;

    // FIX: Ajusta estos valores según el timing de tu animación de ataque
    [Header("Weapon Timing")]
    public float weaponActivationDelay = 0.3f;   // segundos tras lanzar el trigger hasta activar el collider
    public float weaponActiveDuration = 0.5f;     // segundos que el collider permanece activo

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
    public System.Action<int, int> OnVidaChanged;

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

        if (weaponObject != null)
            weaponObject.SetActive(false);

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            OnVidaChanged += gm.UpdateHealthBarGuardian; // necesitas hacer el método público
        }
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

    // COMPORTAMIENTO DE PATRULLA
    public void PatrolBehaviour()
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

    // MOVERSE A UN PUNTO ALEATORIO DENTRO DEL RADIO DE PATRULLA
    public void MoveToRandomPoint()
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
        if (!canInteract) return;
        if (currentState != GuardianState.Patrolling) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionDistance)
        {
            StartGreeting();
        }
    }

    void StartGreeting()
    {
        if (!canInteract) return;

        canInteract = false;

        currentState = GuardianState.Greeting;
        agent.isStopped = true;
        SetWalk(false);

        animator.SetBool("isGreeting", true);

        if (playerController != null)
            playerController.EnterDialogue();

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
            PompompurinController playerRef = FindObjectOfType<PompompurinController>();
            combatManager.StartCombat(this, playerRef);
        }

        if (weaponObject != null)
            weaponObject.SetActive(true);

        StartCombat();
    }

    public void StartCombat()
    {
        Debug.Log("Guardian: Iniciando combate");

        animator.SetBool("InCombat", true);
        canReceiveDamage = true;
        attackTimer = 0f;

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
        if (currentState != GuardianState.Combat &&
            currentState != GuardianState.AllyDefending)
            return;

        isAttacking = true;
        attackTimer = 0f;

        animator.SetTrigger("Attack");

        Debug.Log("Guardian: Ejecutando ataque");

        // Activar el collider del arma con delay para sincronizar con la animación
        StartCoroutine(ActivateWeaponCollider(weaponActivationDelay, weaponActiveDuration));

        // Resetear isAttacking tras el tiempo entre ataques
        StartCoroutine(AttackCooldown());
    }

    IEnumerator ActivateWeaponCollider(float delayBeforeEnable, float duration)
    {
        yield return new WaitForSeconds(delayBeforeEnable);

        // Resetear hasHit antes de cada swing para que cada ataque pueda hacer daño
        if (weaponCollider != null)
        {
            GuardianWeaponCollider gwc = weaponCollider.GetComponent<GuardianWeaponCollider>();
            if (gwc != null) gwc.ResetHit();
        }

        EnableWeaponCollider();

        yield return new WaitForSeconds(duration);
        DisableWeaponCollider();
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
        animator.SetBool("Die", false);
        agent.isStopped = false;
        canReceiveDamage = false;

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

        StartCoroutine(ResetInteraction());
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
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log($"Guardian vida: {currentHealth}/{maxHealth}");
        OnVidaChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
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

        MoveToRandomPoint();
        StartCoroutine(ResetInteraction());
    }

    IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
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

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        isAttacking = false;
    }
}

public class GuardianWeaponCollider : MonoBehaviour
{
    private GuardianController guardianController;
    private CombatManager combatManager;

    [SerializeField] private int damage = 10;
    private bool hasHit = false;

    void Start()
    {
        guardianController = GetComponentInParent<GuardianController>();
        combatManager = FindObjectOfType<CombatManager>();

        if (guardianController == null)
        {
            Debug.LogError($"GuardianWeaponCollider en {gameObject.name} no encontró GuardianController en el padre!");
        }

        if (combatManager == null)
        {
            Debug.LogError("No se encontró CombatManager en la escena!");
        }
    }
    void OnEnable()
    {
        hasHit = false;
    }

    public void ResetHit()
    {
        hasHit = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (guardianController == null || combatManager == null) return;
        if (hasHit) return;

        if (!guardianController.IsAlly())
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"¡Lanza del Guardian golpeó a {other.name}!");
                combatManager.OnGuardianHit(damage);
                hasHit = true;
                guardianController.NotifyWeaponHit();
            }
        }
        else
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log($"¡Guardian aliado golpeó a enemigo: {other.name}!");
                guardianController.NotifyWeaponHit();
                hasHit = true;
            }
        }
    }
}

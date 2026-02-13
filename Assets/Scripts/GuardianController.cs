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

    //Patrulla
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

    public GameObject weaponObject;
    public Collider weaponCollider;
    public int weaponDamage = 15; // Daño de la lanza

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

    void Start()
    {
        if (weaponCollider == null && weaponObject != null)
        {
            weaponCollider = weaponObject.GetComponent<Collider>();
        }

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;

            // Agregar componente de detección de golpe a la lanza
            if (weaponCollider.GetComponent<GuardianWeaponCollider>() == null)
            {
                weaponCollider.gameObject.AddComponent<GuardianWeaponCollider>();
            }
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

    public void EnableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
            Debug.Log("Lanza activada - puede hacer daño");
        }
    }

    public void DisableWeaponCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            Debug.Log("Lanza desactivada - no puede hacer daño");
        }
    }

    // Método público para que el collider de la lanza notifique golpes
    public void NotifyWeaponHit()
    {
        if (combatManager != null)
        {
            combatManager.OnGuardianHit(weaponDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Guardian recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");

        animator.SetTrigger("GolpeP");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Guardian derrotado!");
        animator.SetBool("Die", true);
        animator.SetBool("InCombat", false);

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

        if (weaponObject != null)
            weaponObject.SetActive(false);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}

// CLASE AUXILIAR PARA EL COLLIDER DE LA LANZA
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
        // Verificar si golpeó a Pompompurin
        if (other.CompareTag("Player") || other.name.Contains("Pompompurin"))
        {
            Debug.Log($"¡Lanza golpeó a {other.name}!");

            if (guardianController != null)
            {
                guardianController.NotifyWeaponHit();
            }
        }
    }
}
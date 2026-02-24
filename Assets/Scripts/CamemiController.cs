using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CamemiController : MonoBehaviour
{

    // ESTADOS DE CAMEMI
    public enum CamemiState
    {
        Patrolling,
        Greeting,
        Talking,
        Combat
    }

    [Header("Referencias")]
    private NavMeshAgent agent; // para movimiento
    private Animator animator; // para animacioneS
    public Transform player; // referencia al jugador para detección y combate
    public LayerMask PlayerMask;
    public DialogueManager dialogueManager; // para manejar diálogos
    public Dialogos dialogoEncuentro; // diálogo inicial al detectar al jugador
    public Dialogos dialogoFinal; //diálogo final al morir Camemi
    public GameObject pompompurin;

    [Header("Combate")]
    public CombatManager combatManager;
    public CamemiHitbox[] hitboxesManos;  // los 2 colliders de puños
    public CamemiHitbox[] hitboxesPatas;  // los 2 colliders de patas
    private int comboCounter = 0;
    private float attackCooldown = 1.2f;
    private float attackTimer;
    private bool isBlocking = false;
    private float blockTimer = 0f;
    private float blockCheckInterval = 2f;
    public bool puedeHacerDaño = false;
    private bool isAttacking;
    private bool canReceiveDamage;

    [Header("Datos de vida Camemi")]
    public int vidaMax = 100;// vida máxima de Camemi
    private int vidaActual; // vida actual de Camemi


    [Header("Daños")]
    public int damageGolpe1 = 12; // daño de los ataques normales (puños)
    public int damageGolpe2 = 22; // daño de los ataques fuertes (patas)

    public float tiempoActivacion = 0.2f;
    public float tiempoDesactivacion = 0.5f;

    private PompompurinController playerController;

    [Header("Patrulla")]
    public float patrolRadius = 10f;
    public float waitTimeBetweenPoints = 2f;

    [Header("Detección")]
    public float detectionRange = 5f;

    private float waitTimer;
    private CamemiState currentState;
    private bool canInteract = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform; 
        pompompurin = GameObject.FindGameObjectWithTag("Player"); 
        playerController = pompompurin.GetComponent<PompompurinController>();

        currentState = CamemiState.Patrolling;

        vidaActual = vidaMax;
        agent.isStopped = false;
        agent.ResetPath();

        MoveToRandomPoint();
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case CamemiState.Patrolling:
                PatrolBehaviour();
                DetectPlayer();
                break;

            case CamemiState.Greeting:
            case CamemiState.Talking:
                LookAtPlayer();
                break;

            case CamemiState.Combat:
                LookAtPlayer();
                CombatBehaviour();
                break;
        }
    }

    // ========================
    // PATRULLA
    // ========================

    void PatrolBehaviour()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            animator.SetBool("Walk", false);
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeBetweenPoints)
            {
                MoveToRandomPoint();
                waitTimer = 0f;
            }
        }
        else
        {
            animator.SetBool("Walk", true);
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // ========================
    // DETECCIÓN
    // ========================

    void DetectPlayer()
    {
        if (!canInteract) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            canInteract = false;
            StartDialogue();
        }
    }

    // ========================
    // DIÁLOGO
    // ========================

    // Este método es llamado por el DialogueManager al iniciar el diálogo
    void StartDialogue()
    {
        agent.isStopped = true; // Detenemos el movimiento
        animator.SetBool("Walk", false); // Detenemos la animación de caminar
        animator.SetBool("InDialogue", true); // Activamos la capa de diálogo para que Camemi mire al jugador
        currentState = CamemiState.Talking; // Cambiamos el estado a hablando
        
        if (dialogoEncuentro != null && dialogueManager != null)
        {
            dialogueManager.StartCamemiDialogue(dialogoEncuentro, this); // Iniciamos el diálogo con Camemi
        }
    }

    // Este método es llamado por el DialogueManager al finalizar el diálogo
    public void EndDialogue()
    {
        currentState = CamemiState.Combat; // Cambiamos el estado a combate

        animator.SetBool("InDialogue", false); // Desactivamos la capa de diálogo

        playerController?.ExitDialogue(); // Le decimos al jugador que el diálogo ha terminado para que pueda volver a moverse
        playerController?.StartCombatAfterDialogue(); // Le decimos al jugador que inicie el combate después del diálogo
        combatManager?.StartCamemiCombat(this, playerController); // Le decimos al CombatManager que inicie el combate con Camemi y el jugador

        //if (manoCollider != null)
        //    manoCollider.SetActive(true);
        agent.isStopped = false; // Reanudamos el movimiento para que Camemi pueda perseguir al jugador
        StartCombat(); // Iniciamos el combate
    }

    // ========================
    // COMBATE
    // ========================

    void StartCombat()
    {
        currentState = CamemiState.Combat;

        animator.SetBool("Combat", true);
        canReceiveDamage = true;

        attackTimer = 0f;
        isAttacking = false;

        Invoke(nameof(ExecuteCombo), 0.5f); 
    }

    void CombatBehaviour()
    {
        if (isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > 2f)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("Walk", true);
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("Walk", false);

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                ExecuteCombo();
            }
        }
    }

    void ExecuteCombo()
    {
        if (isAttacking) return;

        isAttacking = true;
        comboCounter++;

        if (comboCounter <= 2)
        {
            animator.SetTrigger("Attack1");
            StartCoroutine(ActivarHitboxes(hitboxesManos));
        }
        else
        {
            animator.SetTrigger("Attack2");
            StartCoroutine(ActivarHitboxes(hitboxesPatas));
            comboCounter = 0;
        }

        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    //public void TryBlock()
    //{
    //    isBlocking = Random.value > 0.7f; // 30% probabilidad

    //    if (isBlocking)
    //        animator.SetTrigger("Block");
    //}

    public void TakeDamage(int damage)
    {
        if (!canReceiveDamage) return;

        if (Random.value > 0.7f)
        {
            animator.SetTrigger("Block");
            return;
        }

        vidaActual -= damage;
        animator.SetTrigger("RecibirGolpe");

        if (vidaActual <= 0)
            Die();
    }

    public void EnableHitboxes()
    {
        Debug.Log("Hitboxes ACTIVADAS");
        foreach (var hitbox in hitboxesManos) hitbox.gameObject.SetActive(true);
        foreach (var hitbox in hitboxesPatas) hitbox.gameObject.SetActive(true);
    }

    public void DisableHitboxes()
    {
        Debug.Log("Hitboxes DESACTIVADAS");
        foreach (var hitbox in hitboxesManos) hitbox.gameObject.SetActive(false);
        foreach (var hitbox in hitboxesPatas) hitbox.gameObject.SetActive(false);
    }

    public void ActivarDaño()
    {
        puedeHacerDaño = true;
    }

    public void DesactivarDaño()
    {
        puedeHacerDaño = false;
    }

    // ========================
    // MIRAR AL JUGADOR
    // ========================

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    // ========================
    // VOLVER A PATRULLAR
    // ========================

    public void ReturnToPatrol()
    {
        currentState = CamemiState.Patrolling;

        agent.isStopped = false;
        agent.ResetPath();

        MoveToRandomPoint();
        canInteract = true;
    }

    // ========================
    // MUERTE
    // ========================
    void Die()
    {
        Debug.Log("Camemi ha sido derrotada");
        animator.SetLayerWeight(1, 0f);
        animator.SetTrigger("Morir");

        agent.isStopped = true;

        CombatManager.Instance.EndCombat(true);

        StartCoroutine(FinalSequence());
    }

    public void EnterCombat()
    {
        animator.SetBool("Combat", true);
    }

    public void ExitCombat()
    {
        animator.SetBool("Combat", false);
    }

    IEnumerator FinalSequence()
    {
        yield return new WaitForSeconds(2.5f);

        if (dialogoFinal != null && dialogueManager != null)
            dialogueManager.StartCamemiDialogue(dialogoFinal, this);

        yield return new WaitForSeconds(5f);

        // UIManager.Instance?.ShowFinalScreen(); // descomenta cuando tengas la pantalla lista
        Debug.Log("Aquí iría la pantalla final");
    }

    private IEnumerator ActivarHitboxes(CamemiHitbox[] grupo)
    {
        yield return new WaitForSeconds(tiempoActivacion);
        foreach (var h in grupo) h.gameObject.SetActive(true);

        yield return new WaitForSeconds(tiempoDesactivacion - tiempoActivacion);
        foreach (var h in grupo) h.gameObject.SetActive(false);
    }
}


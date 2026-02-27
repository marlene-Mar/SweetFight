using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Controlador principal para el NPC/Enemigo "Camemi". 
/// Gestiona estados de patrulla, diálogo, combate y ciclo de vida.
/// </summary>
public class CamemiController : MonoBehaviour
{
    // ── ENUMS Y ESTADOS ─────────────────────────────────
    public enum CamemiState { Patrolling, Greeting, Talking, Combat }
    private CamemiState currentState;

    // ── REFERENCIAS DE COMPONENTES ──────────────────────
    [Header("Referencias")]
    private NavMeshAgent agent;
    private Animator animator;
    public Transform player;
    public LayerMask PlayerMask;
    public DialogueManager dialogueManager;
    public GameObject pompompurin;
    private PompompurinController playerController;

    [Header("Diálogos")]
    public Dialogos dialogoEncuentro;
    public Dialogos dialogoFinal;     // Al agotarse el tiempo
    public Dialogos dialogoVictoria;  // Al ser derrotada por el jugador

    // ── SISTEMA DE COMBATE ──────────────────────────────
    [Header("Combate")]
    public CombatManager combatManager;
    public CamemiHitbox[] hitboxesManos;  // Colliders para ataques rápidos
    public CamemiHitbox[] hitboxesPatas;  // Colliders para ataques fuertes
    private int comboCounter = 0;

    [Header("Tiempos de ataque")]
    public float attackCooldown = 1.8f;   // Tiempo entre combos
    public float tiempoActivacion = 0.3f; // Delay antes de activar hitbox (anticipación)
    public float tiempoHitbox = 0.35f;    // Duración de la ventana de daño

    private float attackTimer;
    private bool isAttacking;
    public bool puedeHacerDaño = false;

    [Header("Daños")]
    public int damageGolpe1 = 12;
    public int damageGolpe2 = 22;

    // ── SISTEMA DE VIDA ─────────────────────────────────
    [Header("Vida")]
    public int vidaMax = 100;
    private int vidaActual;
    private bool isDead = false;
    private bool canReceiveDamage;

    // Evento para actualizar UI (GameManager/HUD)
    public System.Action<int, int> OnVidaChanged;
    public int VidaActual => vidaActual;
    public int VidaMax => vidaMax;

    // ── NAVEGACIÓN Y DETECCIÓN ──────────────────────────
    [Header("Patrulla")]
    public float patrolRadius = 10f;
    public float waitTimeBetweenPoints = 2f;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private float waitTimer;

    [Header("Detección")]
    public float detectionRange = 5f;
    public float combatDistance = 2f;
    private bool canInteract = true;

    // ── MÉTODOS DE INICIALIZACIÓN ───────────────────────

    void Start()
    {
        // Guardar estado inicial para Resets
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Localizar al jugador en la escena
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            pompompurin = playerObj;
            playerController = playerObj.GetComponent<PompompurinController>();
        }

        vidaActual = vidaMax;
        OnVidaChanged?.Invoke(vidaActual, vidaMax);
        currentState = CamemiState.Patrolling;

        // Inicializar daño en los scripts de las hitboxes
        foreach (var h in hitboxesManos) if (h != null) h.damage = damageGolpe1;
        foreach (var h in hitboxesPatas) if (h != null) h.damage = damageGolpe2;

        DisableAllHitboxes();
        agent.isStopped = false;
        MoveToRandomPoint();
    }

    // ── BUCLE PRINCIPAL ─────────────────────────────────

    void Update()
    {
        if (isDead || player == null) return;

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

    // ── LÓGICA DE PATRULLA ──────────────────────────────

    void PatrolBehaviour()
    {
        // Si llegó al destino
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
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ── LÓGICA DE DETECCIÓN Y DIÁLOGO ───────────────────

    void DetectPlayer()
    {
        if (!canInteract) return;
        // Si el jugador entra en el rango de detección, inicia charla
        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            canInteract = false;
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        agent.isStopped = true;
        animator.SetBool("Walk", false);
        animator.SetBool("Combat", false);
        animator.SetBool("InDialogue", true);
        currentState = CamemiState.Talking;

        if (dialogoEncuentro != null && dialogueManager != null)
            dialogueManager.StartCamemiDialogue(dialogoEncuentro, this);
    }

    public void EndDialogue()
    {
        animator.SetBool("InDialogue", false);

        if (isDead || currentState == CamemiState.Patrolling)
        {
            playerController?.ExitDialogue();
            return;
        }

        // Transición de Diálogo a Combate
        playerController?.ExitDialogue();
        playerController?.StartCombatAfterDialogue();
        combatManager?.StartCamemiCombat(this, playerController);
        agent.isStopped = false;
        StartCombat();
    }

    // ── LÓGICA DE COMBATE ───────────────────────────────

    void StartCombat()
    {
        currentState = CamemiState.Combat;
        animator.SetLayerWeight(1, 1f); // Activar capa de animación de combate
        animator.SetBool("Combat", true);
        canReceiveDamage = true;
        attackTimer = attackCooldown;
        isAttacking = false;

        OnVidaChanged?.Invoke(vidaActual, vidaMax);
    }

    void CombatBehaviour()
    {
        if (isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > combatDistance)
        {
            // Acercarse al jugador
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("Walk", true);
        }
        else
        {
            // En rango de ataque
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

        // Alternancia de ataques: 2 rápidos, 1 fuerte
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

        StartCoroutine(AttackCooldownRoutine());
    }

    IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown * 0.85f);
        isAttacking = false;
    }

    // ── GESTIÓN DE DAÑO Y HITBOXES ──────────────────────

    IEnumerator ActivarHitboxes(CamemiHitbox[] grupo)
    {
        yield return new WaitForSeconds(tiempoActivacion);

        foreach (var h in grupo)
            if (h != null) h.gameObject.SetActive(true);

        yield return new WaitForSeconds(tiempoHitbox);

        foreach (var h in grupo)
            if (h != null) h.gameObject.SetActive(false);
    }

    void DisableAllHitboxes()
    {
        foreach (var h in hitboxesManos) if (h != null) h.gameObject.SetActive(false);
        foreach (var h in hitboxesPatas) if (h != null) h.gameObject.SetActive(false);
    }

    public bool CanReceiveDamage() => canReceiveDamage;

    /// <summary>
    /// Recibe daño genérico y gestiona la probabilidad de bloqueo.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!canReceiveDamage) return;

        // Probabilidad de bloqueo (30%) si tiene suficiente vida
        float porcentajeVida = (float)vidaActual / vidaMax;
        if (porcentajeVida > 0.3f && Random.value > 0.7f)
        {
            animator.SetTrigger("Block");
            return;
        }

        vidaActual = Mathf.Max(0, vidaActual - damage);
        OnVidaChanged?.Invoke(vidaActual, vidaMax);
        animator.SetTrigger("RecibirGolpe");

        if (vidaActual <= 0) Die();
    }

    /// <summary>
    /// Daño específico provocado por la entidad 'Guardian'.
    /// </summary>
    public void TakeDamageFromGuardian(int damage)
    {
        if (isDead) return;
        if (CombatManager.Instance != null && !CombatManager.Instance.IsInCombat) return;

        vidaActual = Mathf.Max(0, vidaActual - damage);
        OnVidaChanged?.Invoke(vidaActual, vidaMax);
        animator.SetTrigger("RecibirGolpe");

        if (vidaActual <= 0) Die();
    }

    // ── ESTADO DE MUERTE Y FINALIZACIÓN ────────────────

    void Die()
    {
        if (isDead) return;
        isDead = true;
        canReceiveDamage = false;
        isAttacking = false;
        currentState = CamemiState.Patrolling;

        animator.SetLayerWeight(1, 0f);
        animator.SetBool("Combat", false);
        animator.SetBool("Walk", false);
        animator.SetTrigger("Morir");

        agent.isStopped = true;
        DisableAllHitboxes();
        StopAllCoroutines();

        CombatManager.Instance?.EndCombat(true);
        playerController?.ExitCombat();

        StartCoroutine(FinalSequence());
    }

    IEnumerator FinalSequence()
    {
        yield return new WaitForSeconds(2.5f);

        if (dialogoVictoria != null && dialogueManager != null)
            dialogueManager.StartCamemiDialogue(dialogoVictoria, this);

        yield return new WaitForSeconds(5f);

        // Mostrar créditos y cerrar el juego
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarCreditos();
            yield return new WaitForSecondsRealtime(5f);
            UIManager.Instance.TerminarJuegoDesdeCreditos();
        }
    }

    // ── UTILIDADES PÚBLICAS Y EVENTOS ──────────────────

    public void EnterCombat() => animator.SetBool("Combat", true);
    public void ExitCombat() => animator.SetBool("Combat", false);

    void LookAtPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    public void ReturnToPatrol()
    {
        currentState = CamemiState.Patrolling;
        animator.SetBool("Combat", false);
        agent.isStopped = false;
        agent.ResetPath();
        MoveToRandomPoint();
        canInteract = true;
    }

    public void OnCombatTimeOut()
    {
        currentState = CamemiState.Patrolling;
        animator.SetBool("Combat", false);
        animator.SetLayerWeight(1, 0f);
        agent.isStopped = false;

        if (dialogoFinal != null && dialogueManager != null)
        {
            animator.SetBool("InDialogue", true);
            dialogueManager.StartCamemiDialogue(dialogoFinal, this);
        }

        StartCoroutine(ResumePatrolAfterTimeout());
    }

    IEnumerator ResumePatrolAfterTimeout()
    {
        yield return new WaitForSeconds(4f);
        animator.SetBool("InDialogue", false);
        ReturnToPatrol();
    }

    // ── PERSISTENCIA DE DATOS (SAVE/LOAD) ───────────────

    [Header("Save Data")]
    public Data gameData;

    public void SaveToData()
    {
        gameData.camemiHealth = vidaActual;
        gameData.camemiDefeated = (vidaActual <= 0);
    }

    public void LoadFromData()
    {
        if (gameData.camemiDefeated)
        {
            gameObject.SetActive(false);
            return;
        }
        vidaActual = gameData.camemiHealth > 0 ? gameData.camemiHealth : vidaMax;
        OnVidaChanged?.Invoke(vidaActual, vidaMax);
    }

    /// <summary>
    /// Restablece a Camemi a su estado original cuando se reinicia
    /// </summary>
    public void ResetCamemi()
    {
        isDead = false;
        vidaActual = vidaMax;
        OnVidaChanged?.Invoke(vidaActual, vidaMax);

        currentState = CamemiState.Patrolling;
        canReceiveDamage = false;
        isAttacking = false;
        canInteract = true;

        animator.Rebind();
        animator.Update(0f);

        agent.Warp(posicionInicial);
        transform.rotation = rotacionInicial;

        gameObject.SetActive(true);
        DisableAllHitboxes();
        StopAllCoroutines();

        agent.isStopped = false;
        MoveToRandomPoint();
    }
}
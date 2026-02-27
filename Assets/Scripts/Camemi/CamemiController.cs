using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CamemiController : MonoBehaviour
{
    public enum CamemiState { Patrolling, Greeting, Talking, Combat }

    [Header("Referencias")]
    private NavMeshAgent agent;
    private Animator animator;
    public Transform player;
    public LayerMask PlayerMask;
    public DialogueManager dialogueManager;
    public Dialogos dialogoEncuentro;
    public Dialogos dialogoFinal;     // Al agotarse el tiempo
    public Dialogos dialogoVictoria;  // Al ser derrotada por el jugador
    public GameObject pompompurin;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    [Header("Combate")]
    public CombatManager combatManager;
    public CamemiHitbox[] hitboxesManos;  // 2 colliders de puños
    public CamemiHitbox[] hitboxesPatas;  // 2 colliders de patas
    private int comboCounter = 0;

    [Header("Tiempos de ataque")]
    public float attackCooldown = 1.8f;   // Tiempo entre combos
    public float tiempoActivacion = 0.3f; // Delay antes de activar hitbox
    public float tiempoHitbox = 0.35f;    // Cuánto tiempo está activa la hitbox

    private float attackTimer;
    private bool isAttacking;
    private bool canReceiveDamage;
    public bool puedeHacerDaño = false;

    [Header("Vida")]
    public int vidaMax = 100;
    private int vidaActual;
    private bool isDead = false;

    // Evento y propiedades públicas para que GameManager pueda suscribirse
    public System.Action<int, int> OnVidaChanged;
    public int VidaActual => vidaActual;
    public int VidaMax => vidaMax;

    [Header("Daños")]
    public int damageGolpe1 = 12;
    public int damageGolpe2 = 22;

    [Header("Patrulla")]
    public float patrolRadius = 10f;
    public float waitTimeBetweenPoints = 2f;

    [Header("Detección")]
    public float detectionRange = 5f;
    public float combatDistance = 2f;

    private float waitTimer;
    private CamemiState currentState;
    private bool canInteract = true;
    private PompompurinController playerController;

    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

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

        // Inyectar daño en hitboxes para que nunca queden en 0
        foreach (var h in hitboxesManos) if (h != null) h.damage = damageGolpe1;
        foreach (var h in hitboxesPatas) if (h != null) h.damage = damageGolpe2;

        DisableAllHitboxes();
        agent.isStopped = false;
        MoveToRandomPoint();
    }

    void Update()
    {
        if (isDead) return;

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

    // ── PATRULLA ──────────────────────────────────

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
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ── DETECCIÓN ─────────────────────────────────

    void DetectPlayer()
    {
        if (!canInteract) return;
        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            canInteract = false;
            StartDialogue();
        }
    }

    // ── DIÁLOGO ───────────────────────────────────

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

        playerController?.ExitDialogue();
        playerController?.StartCombatAfterDialogue();
        combatManager?.StartCamemiCombat(this, playerController);
        agent.isStopped = false;
        StartCombat();
    }

    // ── COMBATE ───────────────────────────────────

    void StartCombat()
    {
        currentState = CamemiState.Combat;
        animator.SetLayerWeight(1, 1f);
        animator.SetBool("Combat", true);
        canReceiveDamage = true;
        attackTimer = attackCooldown;
        isAttacking = false;

        // FIX: forzar refresco de barra de vida de Camemi al iniciar combate
        OnVidaChanged?.Invoke(vidaActual, vidaMax);
    }

    void CombatBehaviour()
    {
        if (isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > combatDistance)
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

        StartCoroutine(AttackCooldownRoutine());
    }

    IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown * 0.85f);
        isAttacking = false;
    }

    // ── HITBOXES ──────────────────────────────────

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

    // ── DAÑO RECIBIDO ─────────────────────────────

    public bool CanReceiveDamage() => canReceiveDamage;

    public void TakeDamage(int damage)
    {
        if (!canReceiveDamage) return;

        // Bloqueo solo si tiene más del 30% de vida
        float porcentajeVida = (float)vidaActual / vidaMax;
        if (porcentajeVida > 0.3f && Random.value > 0.7f)
        {
            animator.SetTrigger("Block");
            return;
        }

        vidaActual = Mathf.Max(0, vidaActual - damage);
        OnVidaChanged?.Invoke(vidaActual, vidaMax);
        animator.SetTrigger("RecibirGolpe");

        Debug.Log($"[Camemi] Vida restante: {vidaActual}");

        Debug.Log("Camemi recibió daño del Guardián: " + damage);
        if (vidaActual <= 0)
            Die();
    }

    // NUEVO: Función exclusiva para el daño del aliado
    public void TakeDamageFromGuardian(int damage)
    {
        if (isDead) return;
        // Si quieres que el Guardián le haga daño incluso antes de hablar con ella,
        // quita la siguiente línea. Si quieres que solo la dañe en combate, déjala.
        if (!canReceiveDamage) return;

        vidaActual = Mathf.Max(0, vidaActual - damage);
        OnVidaChanged?.Invoke(vidaActual, vidaMax);

        animator.SetTrigger("RecibirGolpe");

        Debug.Log($"[Camemi] Recibió {damage} de daño del Guardián! Vida: {vidaActual}/{vidaMax}");

        if (vidaActual <= 0)
            Die();
    }

    // ── UTILS PÚBLICOS ────────────────────────────

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

    // ── TIMEOUT DE COMBATE ───────────────────────

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

    // ── MUERTE ────────────────────────────────────

    void Die()
    {
        if (isDead) return; // ✅ Evita llamarse dos veces
        isDead = true;
        canReceiveDamage = false;
        isAttacking = false;
        currentState = CamemiState.Patrolling; // ✅ Saca del estado Combat para detener CombatBehaviour

        Debug.Log("Camemi ha sido derrotada");
        animator.SetLayerWeight(1, 0f);
        animator.SetBool("Combat", false);
        animator.SetBool("Walk", false);
        animator.SetTrigger("Morir");

        agent.isStopped = true;
        DisableAllHitboxes();
        StopAllCoroutines(); // ✅ Detiene cualquier ataque en curso

        CombatManager.Instance?.EndCombat(true);

        // ✅ Notificar al jugador que salga del combate
        playerController?.ExitCombat();

        StartCoroutine(FinalSequence());
    }

    IEnumerator FinalSequence()
    {
        // ✅ Esperar animación de muerte
        yield return new WaitForSeconds(2.5f);

        // ✅ Mostrar diálogo de victoria
        if (dialogoVictoria != null && dialogueManager != null)
            dialogueManager.StartCamemiDialogue(dialogoVictoria, this);

        // ✅ Esperar que termine el diálogo
        yield return new WaitForSeconds(5f);

        // ✅ Mostrar créditos
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarCreditos();
            yield return new WaitForSecondsRealtime(5f);
            UIManager.Instance.TerminarJuegoDesdeCreditos();
        }
        else
            Debug.LogError("UIManager.Instance no encontrado!");
    }

    // ── SAVE DATA ─────────────────────────────────

    [Header("Save Data")]
    public Data gameData;

    public void SaveToData()
    {
        gameData.camemiHealth = vidaActual;
        gameData.camemiDefeated = vidaActual <= 0;
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

    public void ResetCamemi()
    {
        isDead = false;
        vidaActual = vidaMax;
        OnVidaChanged?.Invoke(vidaActual, vidaMax);

        currentState = CamemiState.Patrolling;
        canReceiveDamage = false;
        isAttacking = false;
        canInteract = true;

        // Reiniciamos las animaciones por completo
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
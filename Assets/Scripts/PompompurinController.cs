using UnityEngine;
using System.Collections;

public class PompompurinController : MonoBehaviour
{
    private CharacterController player;
    private Animator pompompurinAnimator;
    private VidaJugador vidaJugador;
  
    private float speed = 3.5f;
    private float gravity = -9.8f;
    private float jumpForce = 4.5f;

    public float smoothTime = 0.3f;
    private Vector3 smoothVelocity;

    public Vector3 spawnPosition;
    public Quaternion spawnRotation;
    private Vector3 moveDirection;
    private Vector3 verticalVelocity;

    private bool isInDialogue = false;

    public Collider[] manoColliders;

    public int damageGolpe1 = 25;
    public int damageGolpe2 = 30;

    public float combatRange = 4.0f;
    public LayerMask enemyLayer;

    public bool inCombat = false;
    public bool isAttacking = false;
    public bool isDead = false;

    private int currentDamage;

    private int golpe1Count = 0;
    private float comboResetTime = 1.5f;
    private float lastHitTime;

    private CombatManager combatManager;

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        pompompurinAnimator = GetComponent<Animator>();
        player = GetComponent<CharacterController>();
        combatManager = FindObjectOfType<CombatManager>();
        vidaJugador = GetComponent<VidaJugador>();

        foreach (var col in manoColliders)
        {
            col.enabled = false;
            if (col.GetComponent<PompompurinHandCollider>() == null)
                col.gameObject.AddComponent<PompompurinHandCollider>();
        }

        if (vidaJugador != null)
            vidaJugador.OnPlayerDead += Die;
    }

    void Update()
    {
        if (isDead) return;

        DetectCombat(); // Verificar si entramos en combate por proximidad
        HandleJump(); // Permitir saltar incluso en combate, pero no moverse

        // Solo manejar ataques si estamos en combate y no en diálogo
        if (inCombat && !isInDialogue) HandleAttackInput();

        // Si estamos en diálogo o atacando, no permitimos movimiento
        moveDirection = Vector3.zero;
        if (!isInDialogue && !isAttacking) HandleMovement();

        ApplyGravity(); // Aplicar gravedad siempre para permitir saltar y caer correctamente
        UpdateAnimator(); // Actualizar animaciones después de todo el movimiento y acciones
    }

    void HandleMovement()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        bool isRunning = Input.GetKey(KeyCode.Q) && (Mathf.Abs(v) > 0.1f || Mathf.Abs(h) > 0.1f);
        float currentSpeed = isRunning ? speed * 2f : speed;

        Vector3 cameraView = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Camera.main.transform.right;

        moveDirection = cameraView * v + cameraRight * h;

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        //Vector3 finalMove = moveDirection * speed;
        //finalMove.y = verticalVelocity.y;
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = verticalVelocity.y;

        player.Move(finalMove * Time.deltaTime);

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 forward = Vector3.SmoothDamp(
                transform.forward,
                moveDirection,
                ref smoothVelocity,
                smoothTime
            );
            transform.forward = forward;
        }

        pompompurinAnimator.SetBool("IsRun", isRunning);
    }

    void UpdateAnimator()
    {
        if (pompompurinAnimator.GetBool("Die")) return;

        pompompurinAnimator.SetBool("IsGrounded", player.isGrounded);

        if (isAttacking || isInDialogue)
        {
            pompompurinAnimator.SetFloat("Speed", 0f);
            pompompurinAnimator.SetBool("IsRun", false);
        }
        else
        {
            pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
        }

        if (vidaJugador != null)
            pompompurinAnimator.SetFloat("life", vidaJugador.vidaActual);
    }

    void HandleJump()
    {
        if (player.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity.y = jumpForce;
            pompompurinAnimator.SetBool("Jump", true);
        }
    }

    void ApplyGravity()
    {
        if (player.isGrounded)
        {
            if (verticalVelocity.y < 0)
                verticalVelocity.y = -2f;
            pompompurinAnimator.SetBool("Jump", false);
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }

    public void EnterDialogue()
    {
        isInDialogue = true;
        pompompurinAnimator.SetBool("InDialogue", true);
        pompompurinAnimator.SetFloat("Speed", 0f);
    }

    public void ExitDialogue()
    {
        isInDialogue = false;
        pompompurinAnimator.SetBool("InDialogue", false);
    }

    public void StartCombatAfterDialogue()
    {
        inCombat = true;
        pompompurinAnimator.SetBool("Combat", true);
    }

    void DetectCombat()
    {
        if (inCombat || isInDialogue) return;

        bool enemiesNearby = Physics.CheckSphere(transform.position, combatRange, enemyLayer);
        if (enemiesNearby)
        {
            inCombat = true;
            pompompurinAnimator.SetBool("Combat", true);
        }
    }

    public void ExitCombat()
    {
        inCombat = false;
        isAttacking = false;
        pompompurinAnimator.SetBool("Combat", false);
    }

    void HandleAttackInput()
    {
        if (isAttacking) return;
        Debug.Log($"HandleAttackInput ejecutándose — inCombat: {inCombat}");

        if (Time.time > lastHitTime + comboResetTime)
            golpe1Count = 0;

        if (Input.GetKeyDown(KeyCode.R) && golpe1Count >= 2)
        {
            currentDamage = damageGolpe2;
            pompompurinAnimator.SetTrigger("Attack2");
            golpe1Count = 0;
            StartCoroutine(AttackWindow(0.2f, 0.35f));
        }
        else if (Input.GetMouseButtonDown(0))
        {
            currentDamage = damageGolpe1;
            pompompurinAnimator.SetTrigger("Attack1");
            golpe1Count++;
            lastHitTime = Time.time;
            StartCoroutine(AttackWindow(0.2f, 0.35f));
        }
    }

    IEnumerator AttackWindow(float delay, float duration)
    {
        isAttacking = true;

        yield return new WaitForSeconds(delay);

        foreach (var col in manoColliders)
            col.enabled = true;

        yield return new WaitForSeconds(duration);

        foreach (var col in manoColliders)
            col.enabled = false;

        yield return new WaitForSeconds(0.05f);
        isAttacking = false;
    }

    public int GetCurrentDamage() => currentDamage;

    void Die()
    {
        if (isDead) return; 
        isDead = true;

        //Detener todo movimiento
        moveDirection = Vector3.zero;
        verticalVelocity = Vector3.zero;
        isAttacking = false;
        isInDialogue = false;

        foreach (var col in manoColliders)
            col.enabled = false;

        pompompurinAnimator.SetFloat("Speed", 0f);
        pompompurinAnimator.SetBool("IsRun", false);
        pompompurinAnimator.SetBool("Combat", false);
        pompompurinAnimator.SetBool("Jump", false);

        //Forzar vida a 0 ANTES de activar Die para que la transición sea consistente
        pompompurinAnimator.SetFloat("life", 0f);
        pompompurinAnimator.SetBool("Die", true);

        Debug.Log("Pompompurin ha muerto!");

        if (combatManager != null)
            combatManager.EndCombat(false);

        StartCoroutine(MuerteYRegresarAlMenu());
    }

    IEnumerator MuerteYRegresarAlMenu()
    {
        float duracionAnimacion = 2.5f;
        yield return new WaitForSeconds(duracionAnimacion);

        if (UIManager.Instance != null)
            UIManager.Instance.MuerteJugador();
        else
            Debug.LogError("UIManager.Instance no encontrado!");
    }

    public void NotifyHitLanded()
    {
        if (combatManager != null && isAttacking)
        {
            Debug.Log($"Pompompurin: Notificando golpe con daño {currentDamage}");
            combatManager.OnPlayerHit(currentDamage);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  PompompurinHandCollider
// ─────────────────────────────────────────────────────────────────────────────
public class PompompurinHandCollider : MonoBehaviour
{
    private PompompurinController playerController;
    private bool hasHit = false;

    void Start()
    {
        playerController = GetComponentInParent<PompompurinController>();

        if (playerController == null)
            Debug.LogError($"PompompurinHandCollider en {gameObject.name} no encontró PompompurinController en el padre!");
    }

    void OnEnable() => hasHit = false;
    void OnDisable() => hasHit = false;

    void OnTriggerEnter(Collider other) => ProcessHit(other);
    void OnTriggerStay(Collider other) => ProcessHit(other);

    void ProcessHit(Collider other)
    {
        if (hasHit) return;
        if (playerController == null || !playerController.isAttacking) return;

        // ── Camemi ────────────────────────────────────────────────────
        CamemiController camemi = other.GetComponent<CamemiController>()
                               ?? other.GetComponentInParent<CamemiController>();
        if (camemi != null && camemi.CanReceiveDamage())
        {
            hasHit = true;
            camemi.TakeDamage(playerController.GetCurrentDamage());
            playerController.NotifyHitLanded();
            Debug.Log($"[Mano] Golpe en Camemi — daño: {playerController.GetCurrentDamage()}");
            return;
        }

        // ── Guardian ──────────────────────────────────────────────────
        GuardianController guardian = other.GetComponent<GuardianController>()
                                   ?? other.GetComponentInParent<GuardianController>();
        if (guardian != null && guardian.CanReceiveDamage())
        {
            hasHit = true;
            int damage = playerController.GetCurrentDamage();

            // BUG 3 FIX: aplicar daño directo al guardián físicamente detectado,
            // independientemente de qué guardián tenga CombatManager como currentEnemy.
            guardian.TakeDamage(damage);

            // Registrar solo estadísticas en CombatManager (sin volver a aplicar daño)
            CombatManager.Instance?.OnPlayerHitGuardian(damage);

            Debug.Log($"[Mano] Golpe en Guardian — daño: {damage}");
        }
    }
}
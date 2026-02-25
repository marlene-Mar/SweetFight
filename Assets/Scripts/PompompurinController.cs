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

    private int currentDamage;

    private int golpe1Count = 0;
    private float comboResetTime = 1.5f;
    private float lastHitTime;

    private CombatManager combatManager;

    void Start()
    {
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
        DetectCombat();
        HandleJump();

        if (inCombat && !isInDialogue)
            HandleAttackInput();

        moveDirection = Vector3.zero;

        if (!isInDialogue && !isAttacking)
            HandleMovement();

        ApplyGravity();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Vector3 cameraView = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Camera.main.transform.right;

        moveDirection = cameraView * v + cameraRight * h;

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        Vector3 finalMove = moveDirection * speed;
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

        // Activar colliders de manos
        foreach (var col in manoColliders)
            col.enabled = true;

        yield return new WaitForSeconds(duration);

        // Desactivar colliders
        foreach (var col in manoColliders)
            col.enabled = false;

        // Liberar isAttacking un poco antes del final de la animación
        // para que combos rápidos no pierdan inputs
        yield return new WaitForSeconds(0.05f);
        isAttacking = false;
    }

    void UpdateAnimator()
    {
        if (pompompurinAnimator.GetBool("Die")) return;

        pompompurinAnimator.SetBool("IsGrounded", player.isGrounded);

        if (isAttacking || isInDialogue)
            pompompurinAnimator.SetFloat("Speed", 0f);
        else
            pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);

        if (vidaJugador != null)
            pompompurinAnimator.SetFloat("life", vidaJugador.vidaActual);
    }

    public int GetCurrentDamage() => currentDamage;

    void Die()
    {
        moveDirection = Vector3.zero;
        verticalVelocity = Vector3.zero;
        isAttacking = false;
        isInDialogue = false;

        foreach (var col in manoColliders)
            col.enabled = false;

        pompompurinAnimator.SetFloat("Speed", 0f);
        pompompurinAnimator.SetBool("Combat", false);
        pompompurinAnimator.SetBool("Jump", false);
        pompompurinAnimator.SetBool("Die", true);

        Debug.Log("Pompompurin ha muerto!");

        if (combatManager != null)
            combatManager.EndCombat(false);

        StartCoroutine(DisableAfterDeath());
    }

    IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(0.1f);
        enabled = false;
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
//  PompompurinHandCollider — detecta colisiones de las manos con enemigos
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
    void OnDisable() => hasHit = false; // Reset al desactivar por combos rápidos

    void OnTriggerEnter(Collider other) => ProcessHit(other);

    // Respaldo: si el enemigo ya está dentro del collider al activarse
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
            playerController.NotifyHitLanded();
            Debug.Log("[Mano] Golpe en Guardian.");
        }
    }
}
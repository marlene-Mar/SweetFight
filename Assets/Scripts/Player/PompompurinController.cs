using UnityEngine;
using System.Collections;

/// <summary>
/// Controlador principal del personaje jugador "Pompompurin".
/// Maneja movimiento, salto, animaciones, combate, diálogos y muerte.
/// </summary>
public class PompompurinController : MonoBehaviour
{
    // ─── REFERENCIAS A COMPONENTES ─────────────────────────────────────────
    private CharacterController player;           // Componente de Unity para mover físicamente al personaje
    private Animator pompompurinAnimator;         // Controla todas las animaciones del personaje
    private VidaJugador vidaJugador;              // Componente que gestiona los puntos de vida del jugador

    // ─── PARÁMETROS DE MOVIMIENTO ──────────────────────────────────────────
    private float speed = 3.5f;                   // Velocidad base de desplazamiento (unidades/segundo)
    private float gravity = -9.8f;                // Fuerza gravitacional aplicada cada frame
    private float jumpForce = 4.5f;               // Fuerza inicial del salto (empuje vertical)

    public float smoothTime = 0.3f;               // Tiempo de suavizado para la rotación del personaje
    private Vector3 smoothVelocity;               // Variable interna usada por SmoothDamp (rotación)

    // ─── POSICIÓN DE REAPARICIÓN (SPAWN) ───────────────────────────────────
    public Vector3 spawnPosition;                 // Posición donde nació el personaje (se usa para respawn)
    public Quaternion spawnRotation;              // Rotación inicial del personaje al nacer

    // ─── VECTORES DE MOVIMIENTO ────────────────────────────────────────────
    private Vector3 moveDirection;                // Dirección horizontal actual de movimiento
    private Vector3 verticalVelocity;             // Velocidad vertical (afectada por gravedad y salto)

    // ─── ESTADO DE DIÁLOGO ─────────────────────────────────────────────────
    private bool isInDialogue = false;            // Bloquea movimiento y ataques mientras hay diálogo activo

    // ─── COMBATE ───────────────────────────────────────────────────────────
    public Collider[] manoColliders;              // Colliders de las manos que detectan impactos al atacar

    public int damageGolpe1 = 12;                 // Daño que inflige el ataque básico (clic izquierdo)

    public float combatRange = 4.0f;              // Radio de detección de enemigos para entrar en combate
    public LayerMask enemyLayer;                  // Capa de Unity que identifica qué objetos son enemigos

    public bool inCombat = false;                 // Indica si el jugador está actualmente en modo combate
    public bool isAttacking = false;              // Verdadero mientras dura la animación/ventana de ataque
    public bool isDead = false;                   // Verdadero cuando el jugador ha muerto

    private int currentDamage;                    // Daño del ataque activo (puede variar si hay varios ataques)

    private float lastHitTime;                    // Marca de tiempo del último golpe (para control de cadencia)

    private CombatManager combatManager;          // Referencia al gestor global de combate de la escena

    // ──────────────────────────────────────────────────────────────────────
    // INICIO
    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Guardar posición y rotación originales para posibles respawns
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        // Obtener referencias a componentes del mismo GameObject
        pompompurinAnimator = GetComponent<Animator>();
        player = GetComponent<CharacterController>();
        combatManager = FindObjectOfType<CombatManager>(); // Buscar en toda la escena
        vidaJugador = GetComponent<VidaJugador>();
        
        // Desactivar todos los colliders de mano al inicio
        // y asegurar que cada mano tenga su componente de detección de golpes
        foreach (var col in manoColliders)
        {
            col.enabled = false; // Se activan solo durante la ventana de golpe
            if (col.GetComponent<PompompurinHandCollider>() == null)
                col.gameObject.AddComponent<PompompurinHandCollider>(); // Agregar si falta
        }

        // Suscribirse al evento de muerte: cuando VidaJugador dispare OnPlayerDead, ejecutar Die()
        if (vidaJugador != null)
            vidaJugador.OnPlayerDead += Die;
    }

    // ──────────────────────────────────────────────────────────────────────
    // ACTUALIZACIÓN POR FRAME
    // Orden de ejecución: detección → salto → ataque → movimiento → gravedad → animaciones
    // ──────────────────────────────────────────────────────────────────────
    void Update()
    {
        // Si el personaje ya murió, no hacer nada más en este frame
        if (isDead) return;

        DetectCombat();   // Verificar si hay enemigos cerca para activar modo combate

        HandleJump();     // Permitir saltar siempre (incluso en combate), pero no caminar

        // Solo procesar input de ataque si estamos en combate y fuera de diálogo
        if (inCombat && !isInDialogue) HandleAttackInput();

        // Reiniciar dirección de movimiento; solo se mueve si no hay diálogo ni ataque activo
        moveDirection = Vector3.zero;
        if (!isInDialogue && !isAttacking) HandleMovement();

        ApplyGravity();   // Aplicar gravedad siempre (permite caer y saltar correctamente)

        UpdateAnimator(); // Actualizar parámetros del Animator con el estado final del frame
    }

    // ──────────────────────────────────────────────────────────────────────
    // MOVIMIENTO
    // Lee ejes de entrada, aplica velocidad relativa a la cámara y rota el personaje.
    // ──────────────────────────────────────────────────────────────────────
    void HandleMovement()
    {
        // Leer input de teclado/joystick (valores entre -1 y 1)
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        // Correr si se mantiene Q y hay input de movimiento
        bool isRunning = Input.GetKey(KeyCode.Q) && (Mathf.Abs(v) > 0.1f || Mathf.Abs(h) > 0.1f);
        float currentSpeed = isRunning ? speed * 2f : speed; // Duplicar velocidad al correr

        // Calcular direcciones relativas a la cámara (ignorando el eje Y)
        Vector3 cameraView = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Camera.main.transform.right;

        // Combinar ejes con las direcciones de la cámara para movimiento relativo a la vista
        moveDirection = cameraView * v + cameraRight * h;

        // Normalizar si la magnitud supera 1 (evita movimiento diagonal más rápido)
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        // Construir vector de movimiento final: dirección horizontal + velocidad vertical (gravedad/salto)
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = verticalVelocity.y;

        // Mover el CharacterController (maneja colisiones automáticamente)
        player.Move(finalMove * Time.deltaTime);

        // Rotar suavemente el personaje en la dirección de movimiento
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 forward = Vector3.SmoothDamp(
                transform.forward,   // Dirección actual del personaje
                moveDirection,       // Dirección objetivo
                ref smoothVelocity,  // Velocidad interna de interpolación
                smoothTime           // Tiempo en segundos para completar la rotación
            );
            transform.forward = forward; // Aplicar la nueva orientación
        }

        // Informar al Animator si el personaje está corriendo
        pompompurinAnimator.SetBool("IsRun", isRunning);
    }

    // ──────────────────────────────────────────────────────────────────────
    // ACTUALIZACIÓN DE ANIMADOR
    // Sincroniza los parámetros del Animator con el estado actual del personaje.
    // ──────────────────────────────────────────────────────────────────────
    void UpdateAnimator()
    {
        // Si ya está ejecutando la animación de muerte, no sobreescribir nada
        if (pompompurinAnimator.GetBool("Die")) return;

        // Informar si el personaje está en el suelo (para transiciones de salto)
        pompompurinAnimator.SetBool("IsGrounded", player.isGrounded);

        if (isAttacking || isInDialogue)
        {
            // Durante ataques o diálogos: forzar velocidad 0 y cancelar carrera
            pompompurinAnimator.SetFloat("Speed", 0f);
            pompompurinAnimator.SetBool("IsRun", false);
        }
        else
        {
            // En movimiento normal: pasar la magnitud del vector de dirección como "velocidad"
            pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
        }

        // Sincronizar vida actual con el Animator (puede activar animaciones de daño/muerte)
        if (vidaJugador != null)
            pompompurinAnimator.SetFloat("life", vidaJugador.vidaActual);
    }

    // ──────────────────────────────────────────────────────────────────────
    // SALTO
    // Solo permite saltar cuando el personaje está en el suelo.
    // ──────────────────────────────────────────────────────────────────────
    void HandleJump()
    {
        if (player.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity.y = jumpForce; // Aplicar impulso hacia arriba
            pompompurinAnimator.SetBool("Jump", true);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // GRAVEDAD
    // Incrementa la velocidad de caída cada frame mientras no esté en el suelo.
    // ──────────────────────────────────────────────────────────────────────
    void ApplyGravity()
    {
        if (player.isGrounded)
        {
            // Pequeña velocidad negativa constante para mantener contacto con el suelo
            if (verticalVelocity.y < 0)
                verticalVelocity.y = -2f;

            pompompurinAnimator.SetBool("Jump", false); // Resetear animación de salto al aterrizar
        }
        else
        {
            // Acumular gravedad con el paso del tiempo (caída libre)
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // GESTIÓN DE DIÁLOGOS
    // Llamadas externamente por el sistema de diálogos para bloquear/desbloquear al jugador.
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Bloquea movimiento y ataques, activa estado de diálogo en el Animator.</summary>
    public void EnterDialogue()
    {
        isInDialogue = true;
        pompompurinAnimator.SetBool("InDialogue", true);
        pompompurinAnimator.SetFloat("Speed", 0f); // Detener animación de caminar
    }

    /// <summary>Desbloquea al jugador una vez que termina el diálogo.</summary>
    public void ExitDialogue()
    {
        isInDialogue = false;
        pompompurinAnimator.SetBool("InDialogue", false);
    }

    /// <summary>
    /// Activa el modo combate inmediatamente después de un diálogo.
    /// Usado cuando un diálogo introduce directamente un combate.
    /// </summary>
    public void StartCombatAfterDialogue()
    {
        inCombat = true;
        pompompurinAnimator.SetBool("Combat", true);
    }

    // ──────────────────────────────────────────────────────────────────────
    // DETECCIÓN DE COMBATE POR PROXIMIDAD
    // Activa el modo combate automáticamente si un enemigo entra en el radio definido.
    // ──────────────────────────────────────────────────────────────────────
    void DetectCombat()
    {
        // No revisar si ya estamos en combate o en diálogo (evita verificaciones innecesarias)
        if (inCombat || isInDialogue) return;

        // CheckSphere devuelve true si hay algún collider en la capa enemyLayer dentro de combatRange
        bool enemiesNearby = Physics.CheckSphere(transform.position, combatRange, enemyLayer);
        if (enemiesNearby)
        {
            inCombat = true;
            pompompurinAnimator.SetBool("Combat", true);
        }
    }

    /// <summary>Desactiva el modo combate. Llamado externamente por CombatManager al terminar el combate.</summary>
    public void ExitCombat()
    {
        inCombat = false;
        isAttacking = false;
        pompompurinAnimator.SetBool("Combat", false);
    }

    // ──────────────────────────────────────────────────────────────────────
    // INPUT DE ATAQUE
    // Lee el clic del ratón y lanza la corrutina de ventana de golpe.
    // ──────────────────────────────────────────────────────────────────────
    void HandleAttackInput()
    {
        // Si ya hay un ataque en curso, no encadenar otro
        if (isAttacking) return;

        Debug.Log($"HandleAttackInput ejecutándose — inCombat: {inCombat}");

        if (Input.GetMouseButtonDown(0)) // Botón izquierdo del ratón
        {
            currentDamage = damageGolpe1;                    // Asignar el daño de este ataque
            pompompurinAnimator.SetTrigger("Attack1");       // Disparar animación de ataque
            lastHitTime = Time.time;                         // Registrar el momento del ataque
            StartCoroutine(AttackWindow(0.2f, 0.35f));       // Iniciar ventana de hitbox
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // CORRUTINA: VENTANA DE ATAQUE
    // Controla el ciclo de vida de la hitbox de la mano:
    //   delay    → tiempo antes de que la hitbox se active (anticipación de la animación)
    //   duration → tiempo que la hitbox permanece activa (puede golpear enemigos)
    // ──────────────────────────────────────────────────────────────────────
    IEnumerator AttackWindow(float delay, float duration)
    {
        isAttacking = true; // Bloquear nuevos ataques y movimiento

        yield return new WaitForSeconds(delay); // Esperar antes de activar la hitbox

        // Activar colliders de la mano y resetear el flag de "ya golpeé" para este ataque
        foreach (var col in manoColliders)
        {
            col.enabled = true;
            var handCollider = col.GetComponent<PompompurinHandCollider>();
            if (handCollider != null) handCollider.ResetHit(); // Permitir un nuevo golpe por ataque
        }

        yield return new WaitForSeconds(duration); // Esperar mientras la hitbox está activa

        // Desactivar colliders de la mano al terminar la ventana
        foreach (var col in manoColliders)
            col.enabled = false;

        yield return new WaitForSeconds(0.05f); // Pequeña pausa antes de permitir otro ataque
        isAttacking = false;
    }

    /// <summary>Devuelve el daño del ataque actualmente en ejecución.</summary>
    public int GetCurrentDamage() => currentDamage;

    // ──────────────────────────────────────────────────────────────────────
    // MUERTE DEL JUGADOR
    // Suscrita al evento OnPlayerDead de VidaJugador.
    // Detiene todo movimiento/acción, activa la animación de muerte
    // y lanza la corrutina que muestra el menú de Game Over.
    // ──────────────────────────────────────────────────────────────────────
    void Die()
    {
        // Evitar ejecutar Die() más de una vez (el evento podría dispararse varias veces)
        if (isDead) return;
        isDead = true;

        // Detener físicas y estado de acción
        moveDirection = Vector3.zero;
        verticalVelocity = Vector3.zero;
        isAttacking = false;
        isInDialogue = false;

        // Desactivar hitboxes de mano por si el jugador muere mientras ataca
        foreach (var col in manoColliders)
            col.enabled = false;

        // Resetear todos los parámetros del Animator a estado neutro
        pompompurinAnimator.SetFloat("Speed", 0f);
        pompompurinAnimator.SetBool("IsRun", false);
        pompompurinAnimator.SetBool("Combat", false);
        pompompurinAnimator.SetBool("Jump", false);

        // Forzar vida a 0 ANTES de activar Die para garantizar una transición correcta en el Animator
        pompompurinAnimator.SetFloat("life", 0f);
        pompompurinAnimator.SetBool("Die", true); // Activar la animación de muerte

        Debug.Log("Pompompurin ha muerto!");

        // Notificar al CombatManager que el combate terminó con derrota
        if (combatManager != null)
            combatManager.EndCombat(false);

        StartCoroutine(MuerteYRegresarAlMenu()); // Esperar animación y mostrar Game Over
    }

    // ──────────────────────────────────────────────────────────────────────
    // CORRUTINA: MUERTE → MENÚ
    // Espera que termine la animación de muerte y luego llama al UIManager
    // para mostrar la pantalla de Game Over.
    // ──────────────────────────────────────────────────────────────────────
    IEnumerator MuerteYRegresarAlMenu()
    {
        float duracionAnimacion = 2.5f;
        yield return new WaitForSeconds(duracionAnimacion); // Esperar la animación de muerte

        if (UIManager.Instance != null)
            UIManager.Instance.MuerteJugador(); // Mostrar pantalla de derrota
        else
            Debug.LogError("UIManager.Instance no encontrado!");
    }

    /// <summary>
    /// Notifica al CombatManager que un golpe aterrizó exitosamente.
    /// Solo se llama si el personaje sigue atacando cuando el collider detecta impacto.
    /// </summary>
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
//  Componente adjunto a cada collider de mano de Pompompurin.
//  Detecta impactos contra enemigos durante la ventana de ataque activa
//  y aplica el daño correspondiente una sola vez por golpe.
// ─────────────────────────────────────────────────────────────────────────────
public class PompompurinHandCollider : MonoBehaviour
{
    private PompompurinController playerController; // Referencia al controlador principal del jugador
    private bool hasHit = false;                    // Garantiza que un mismo ataque solo golpee una vez

    /// <summary>Permite que este collider registre un nuevo golpe (llamado antes de cada ataque).</summary>
    public void ResetHit() => hasHit = false;

    void Start()
    {
        // Buscar el controlador en el GameObject padre (la mano es hija del personaje)
        playerController = GetComponentInParent<PompompurinController>();

        if (playerController == null)
            Debug.LogError($"PompompurinHandCollider en {gameObject.name} no encontró PompompurinController en el padre!");
    }

    // Resetear el flag cada vez que el collider se activa o desactiva
    // para evitar golpes residuales de ataques anteriores
    void OnEnable() => hasHit = false;
    void OnDisable() => hasHit = false;

    // Detectar tanto la entrada al trigger como la permanencia (por si el enemigo ya estaba dentro)
    void OnTriggerEnter(Collider other) => ProcessHit(other);
    void OnTriggerStay(Collider other) => ProcessHit(other);

    // ──────────────────────────────────────────────────────────────────────
    // PROCESAR IMPACTO
    // Verifica el tipo de enemigo golpeado y aplica daño según corresponda.
    // ──────────────────────────────────────────────────────────────────────
    void ProcessHit(Collider other)
    {
        // Evitar dobles golpes en el mismo ataque
        if (hasHit) return;
        // Verificar que el controlador existe y que el ataque sigue activo
        if (playerController == null || !playerController.isAttacking) return;

        // ── Intentar golpear a Camemi ──────────────────────────────────
        // Buscar el componente tanto en el objeto directo como en sus padres
        CamemiController camemi = other.GetComponent<CamemiController>()
                               ?? other.GetComponentInParent<CamemiController>();
        if (camemi != null && camemi.CanReceiveDamage())
        {
            hasHit = true; // Marcar como golpe registrado (evitar golpes repetidos)
            camemi.TakeDamage(playerController.GetCurrentDamage());    // Aplicar daño al enemigo
            playerController.NotifyHitLanded();                        // Notificar al CombatManager
            Debug.Log($"[Mano] Golpe en Camemi — daño: {playerController.GetCurrentDamage()}");
            return; // Salir para no procesar más colisiones en este hit
        }

        // ── Intentar golpear a Guardian ────────────────────────────────
        GuardianController guardian = other.GetComponent<GuardianController>()
                                   ?? other.GetComponentInParent<GuardianController>();
        if (guardian != null && guardian.CanReceiveDamage())
        {
            hasHit = true;
            int damage = playerController.GetCurrentDamage();

            // CORRECCIÓN (Bug #3): Aplicar daño directamente al guardián detectado físicamente,
            // independientemente de cuál guardián tenga CombatManager como enemigo activo.
            // Esto evita que se aplique daño al guardián equivocado.
            guardian.TakeDamage(damage);

            // Solo reportar estadísticas al CombatManager, sin volver a aplicar daño
            CombatManager.Instance?.OnPlayerHitGuardian(damage);

            Debug.Log($"[Mano] Golpe en Guardian — daño: {damage}");
        }
    }
}
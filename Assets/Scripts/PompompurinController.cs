using UnityEngine;
using System.Collections;

public class PompompurinController : MonoBehaviour
{
    private CharacterController player;
    private Animator pompompurinAnimator;

    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpForce = 4.5f;

    public float smoothTime = 0.3f;
    private Vector3 smoothVelocity;

    private Vector3 moveDirection;
    private Vector3 verticalVelocity;

    private bool isInDialogue = false;

    public Collider[] manoColliders;

    public int damageGolpe1 = 10;
    public int damageGolpe2 = 15;

    public float combatRange = 2.0f;
    public LayerMask enemyLayer;

    public bool inCombat = false;
    public bool isAttacking = false;

    private int currentDamage;
    public int life = 100;

    void Start()
    {
        pompompurinAnimator = GetComponent<Animator>();
        player = GetComponent<CharacterController>();

        foreach (var col in manoColliders)
            col.enabled = false;
    }

    void Update()
    {
        DetectCombat();
        HandleJump();

        if (inCombat && !isInDialogue)
        {
            HandleAttackInput();
        }

        moveDirection = Vector3.zero;

        if (!isInDialogue && !isAttacking)
        {
            HandleMovement();
        }

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
        pompompurinAnimator.SetBool("InCombat", true);
    }

    void DetectCombat()
    {
        if (inCombat || isInDialogue) return;

        bool enemiesNearby = Physics.CheckSphere(transform.position, combatRange, enemyLayer);

        if (enemiesNearby)
        {
            inCombat = true;
            pompompurinAnimator.SetBool("InCombat", true);
        }
    }

    public void ExitCombat()
    {
        inCombat = false;
        isAttacking = false;
        pompompurinAnimator.SetBool("InCombat", false);
    }

    void HandleAttackInput()
    {
        if (isAttacking) return;

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.R))
        {
            currentDamage = damageGolpe2;
            pompompurinAnimator.SetTrigger("Attack2");
            StartCoroutine(AttackWindow(0.25f, 0.15f));
        }
        else if (Input.GetMouseButtonDown(0))
        {
            currentDamage = damageGolpe1;
            pompompurinAnimator.SetTrigger("Attack1");
            StartCoroutine(AttackWindow(0.2f, 0.12f));
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

        isAttacking = false;
    }

    void UpdateAnimator()
    {
        pompompurinAnimator.SetBool("IsGrounded", player.isGrounded);

        if (isAttacking || isInDialogue)
        {
            pompompurinAnimator.SetFloat("Speed", 0f);
        }
        else
        {
            pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
        }

        pompompurinAnimator.SetFloat("life", life);
    }


    public int GetCurrentDamage()
    {
        return currentDamage;
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        pompompurinAnimator.SetTrigger("RecibirGolpe"); // Agregar trigger para recibir daño

        Debug.Log($"Pompompurin recibió {damage} de daño. Salud: {life}/100");

        if (life <= 0)
        {
            life = 0;
            Die();
        }
    }

    void Die()
    {
        pompompurinAnimator.SetBool("IsDead", true);
        Debug.Log("Pompompurin ha muerto!");
        enabled = false;
    }
}
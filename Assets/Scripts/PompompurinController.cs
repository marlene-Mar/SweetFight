using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PompompurinController : MonoBehaviour
{
    private CharacterController player;
    private Animator pompompurinAnimator;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpForce = 4.5f;

    public float smoothTime = 0.3f;
    private Vector3 smoothVelocity;

    private Vector3 moveDirection;
    private Vector3 verticalVelocity;
    private Vector3 cameraView;

    private bool isInDialogue = false;
    private bool isDancing = false;
    private bool isWaitingCombat = false;

    public Collider[] manoColliders;

    public int damageGolpe1 = 10;
    public int damageGolpe2 = 15;

    public float combatRange = 2.0f;
    public LayerMask enemyLayer;

    public bool inCombat = false;
    public bool isAttacking = false;

    private int currentDamage;

    void Start()
    {
        pompompurinAnimator = GetComponent<Animator>();
        player = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleDanceInput();
        HandleAttackInput();

        moveDirection = Vector3.zero;

        if (!isDancing && !isInDialogue && !isWaitingCombat)
        {
            HandleMovement();
            HandleJump();
        }

        ApplyGravity();
        UpdateAnimator();
        DetectCombat();
    }

    void HandleMovement()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        cameraView = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
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
            pompompurinAnimator.SetTrigger("Jump");
        }
    }

    void ApplyGravity()
    {
        if (player.isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;
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

    public void EnterCombatPreparation()
    {
        isWaitingCombat = true;
        pompompurinAnimator.SetBool("WaitFight", true);
    }

    public void StartCombat()
    {
        isWaitingCombat = false;
        inCombat = true;

        pompompurinAnimator.SetBool("WaitFight", false);
        pompompurinAnimator.SetBool("InCombat", true);
    }

    void DetectCombat()
    {
        inCombat = Physics.CheckSphere(transform.position, combatRange, enemyLayer);
    }

    void HandleAttackInput()
    {
        if (isAttacking) return;
        if (!inCombat) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.R))
            {
                currentDamage = damageGolpe2;
                pompompurinAnimator.SetTrigger("Attack2");
                StartCoroutine(AttackWindow(0.25f, 0.15f));
            }
            else
            {
                currentDamage = damageGolpe1;
                pompompurinAnimator.SetTrigger("Attack1");
                StartCoroutine(AttackWindow(0.2f, 0.12f));
            }
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

    void HandleDanceInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isDancing = true;
            pompompurinAnimator.SetTrigger("Dance");
        }
    }

    public void StopDance()
    {
        isDancing = false;
    }

    void UpdateAnimator()
    {
        pompompurinAnimator.SetBool("IsGrounded", player.isGrounded);

        if (!isInDialogue)
            pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
    }

    public int GetCurrentDamage()
    {
        return currentDamage;
    }
}
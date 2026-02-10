using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PompompurinController : MonoBehaviour
{
    private CharacterController player;
    private Animator pompompurinAnimator;
    private AnimatorStateInfo stateInfo;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpForce = 4.5f;

    public float smoothTime = 0.3f;
    private Vector3 smoothVelocity;

    private Vector3 cameraView;
    private Vector3 velocity = Vector3.zero;
    private Vector3 moveDirection;
    private Vector3 verticalVelocity;

    private bool isInDialogue = false;
    private bool isDancing = false;

    public Collider[] manoColliders;
    public int damageGolpe1 = 15;
    public int damageGolpe2 = 20;
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
        stateInfo = pompompurinAnimator.GetCurrentAnimatorStateInfo(0);

        HandleDanceInput();
        HandleAttackInput();

        moveDirection = Vector3.zero;

        if (!isDancing && !isInDialogue)
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

        player.Move(moveDirection * speed * Time.deltaTime);

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

    // ================= JUMP =================
    void HandleJump()
    {
        if (player.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity.y = jumpForce;
            pompompurinAnimator.SetTrigger("Jump");
        }
    }

    // ================= GRAVITY =================
    void ApplyGravity()
    {
        if (player.isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;
        //player.Move(verticalVelocity * Time.deltaTime);
    }

    // ================= DIALOGUE =================
    public void EnterDialogue()
    {
        isInDialogue = true;
        moveDirection = Vector3.zero;
        verticalVelocity = Vector3.zero;
        pompompurinAnimator.SetBool("InDialogue", true);
        pompompurinAnimator.SetFloat("Speed", 0f);
    }

    public void ExitDialogue()
    {
        isInDialogue = false;
        pompompurinAnimator.SetBool("InDialogue", false);
    }

    // ================= ATTACK =================
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

    public void TakeDamage(int damage)
    {
        GameManager hp = GetComponent<GameManager>();
        if (hp != null)
            hp.TakeDamage(damage);
    }


    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ================= DANCE =================
    void HandleDanceInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && stateInfo.IsName("Idle"))
        {
            isDancing = true;
            moveDirection = Vector3.zero;
            pompompurinAnimator.SetTrigger("Dance");
        }
    }

    // Llamar desde un Animation Event al final del baile
    public void StopDance()
    {
        isDancing = false;
    }

    // ================= ANIMATOR =================
    void UpdateAnimator()
    {
        //pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
        pompompurinAnimator.SetBool("IsGrounded", player.isGrounded);
        if (!isInDialogue)
        {
            pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
        }
    }

    public int GetCurrentDamage()
    {
        return currentDamage;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public void CalibrateForward()
    {
        Debug.Log("Calibrando forward");
        transform.forward = cameraView;
    }

    public void SmoothForward()
    {
        Debug.Log("Suavizando forward");
        transform.forward = Vector3.SmoothDamp(transform.forward,
                                                   cameraView,
                                                   ref velocity,
                                                   smoothTime);
    }
}

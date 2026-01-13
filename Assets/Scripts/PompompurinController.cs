using UnityEngine;

public class PompompurinController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Key Bindings")]
    public KeyCode moveForward = KeyCode.W;
    public KeyCode moveBack = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode danceKey = KeyCode.B;

    [Header("Combat")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode comboKey = KeyCode.Mouse1;

    private Animator animator;
    private Rigidbody rb;

    private bool isGrounded = true;
    private bool isDead = false;

    //void Start()
    //{
    //    animator = GetComponent<Animator>();
    //    rb = GetComponent<Rigidbody>();
    //}

    //void Update()
    //{
    //    if (isDead) return;

    //    HandleMovement();
    //    HandleJump();
    //    HandleCombat();
    //    HandleDance();
    //}

    // ================= MOVEMENT =================
    void HandleMovement()
    {
        float h = 0;
        float v = 0;

        if (Input.GetKey(moveForward)) v += 1;
        if (Input.GetKey(moveBack)) v -= 1;
        if (Input.GetKey(moveRight)) h += 1;
        if (Input.GetKey(moveLeft)) h -= 1;

        Vector3 input = new Vector3(h, 0, v).normalized;

        animator.SetFloat("Speed", input.magnitude, 0.1f, Time.deltaTime);
        animator.SetFloat("Direction", h, 0.1f, Time.deltaTime);

        if (input.magnitude > 0.1f)
        {
            transform.forward = Vector3.Lerp(
                transform.forward,
                input,
                Time.deltaTime * rotationSpeed
            );

            transform.position += transform.forward * walkSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetTrigger("StopWalk");
        }
    }

    // ================= JUMP =================
    void HandleJump()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                7f,
                rb.linearVelocity.z
            );

            animator.SetTrigger("Jump");
            isGrounded = false;
        }
    }

    // ================= COMBAT =================
    void HandleCombat()
    {
        if (Input.GetKeyDown(attackKey))
        {
            animator.SetBool("InCombat", true);
            animator.SetTrigger("Attack1");
        }

        if (Input.GetKeyDown(comboKey))
        {
            animator.SetTrigger("Attack2");
        }
    }

    // ================= DANCE =================
    void HandleDance()
    {
        if (Input.GetKeyDown(danceKey))
        {
            animator.SetTrigger("Dance");
        }
    }

    // ================= EXTERNAL EVENTS =================
    public void ReceiveHit()
    {
        animator.SetTrigger("Hit");
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);
        animator.SetTrigger("Die");
    }

    // ================= COLLISION =================
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("IsGrounded", true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            animator.SetBool("IsGrounded", false);
        }
    }
}

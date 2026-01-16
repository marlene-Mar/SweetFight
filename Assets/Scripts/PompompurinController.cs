using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private bool isDancing = false;

    void Start()
    {
        pompompurinAnimator = GetComponent<Animator>();
        player = GetComponent<CharacterController>();
    }

    void Update()
    {
        stateInfo = pompompurinAnimator.GetCurrentAnimatorStateInfo(0);

        HandleDanceInput();

        if (!isDancing)
        {
            HandleMovement();
            HandleJump();
        }

        ApplyGravity();
        UpdateAnimator();

        //float v = Input.GetAxis("Vertical");
        //float h = Input.GetAxis("Horizontal");

        //cameraView = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        //Vector3 cameraRight = Camera.main.transform.right;

        //moveDirection = cameraView * v + cameraRight * h;

        //if (moveDirection.magnitude > 1f)
        //    moveDirection.Normalize();

        //player.Move(moveDirection * speed * Time.deltaTime);

        //if (player.isGrounded && verticalVelocity.y < 0)
        //    verticalVelocity.y = -2f;

        //verticalVelocity.y += gravity * Time.deltaTime;
        //player.Move(verticalVelocity * Time.deltaTime);

        //if (moveDirection.magnitude > 0.1f)
        //{
        //    Vector3 forward = Vector3.SmoothDamp(
        //        transform.forward,
        //        moveDirection,
        //        ref smoothVelocity,
        //        smoothTime
        //    );
        //    transform.forward = forward;
        //}

        //pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
        //pompompurinAnimator.SetFloat("Direction", h);
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

        verticalVelocity.y += gravity * Time.deltaTime;
        player.Move(verticalVelocity * Time.deltaTime);
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
        pompompurinAnimator.SetFloat("Speed", moveDirection.magnitude);
        pompompurinAnimator.SetBool("IsGrounded", player.isGrounded);
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

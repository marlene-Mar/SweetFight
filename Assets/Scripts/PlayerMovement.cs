using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController player;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpForce = 4.5f;

    public float smoothTime = 0.3f;
    private Vector3 smoothVelocity;

    private Vector3 moveDirection;
    private Vector3 verticalVelocity;
    private Vector3 cameraView;

    void Start()
    {
        player = GetComponent<CharacterController>();
    }

    public void HandleMovement()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (Mathf.Abs(v) < 0.1f) v = 0;
        if (Mathf.Abs(h) < 0.1f) h = 0;

        cameraView = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;

        moveDirection = cameraView * v + cameraRight * h;

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        Vector3 finalMove = moveDirection * speed;
        finalMove += verticalVelocity;

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

        ApplyGravity();
    }

    public void HandleJump()
    {
        if (player.isGrounded && Input.GetKeyDown(KeyCode.Space))
            verticalVelocity.y = jumpForce;
    }

    void ApplyGravity()
    {
        if (player.isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;
    }

    public float GetMoveMagnitude()
    {
        return moveDirection.magnitude;
    }

    public bool IsGrounded()
    {
        return player.isGrounded;
    }
}
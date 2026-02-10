using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateAnimator(float moveMagnitude, bool grounded)
    {
        if (moveMagnitude < 0.05f)
            moveMagnitude = 0;

        animator.SetFloat("Speed", moveMagnitude);
        animator.SetBool("IsGrounded", grounded);
    }
}
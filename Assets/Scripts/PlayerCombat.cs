using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;

    public Collider[] manoColliders;
    public int damageGolpe1 = 15;
    public int damageGolpe2 = 20;

    public float combatRange = 2f;
    public LayerMask enemyLayer;

    private bool inCombat;
    private bool isAttacking;
    private int currentDamage;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        DetectCombat();
    }

    void DetectCombat()
    {
        inCombat = Physics.CheckSphere(transform.position, combatRange, enemyLayer);
    }

    public void HandleAttackInput()
    {
        if (isAttacking || !inCombat) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.R))
            {
                currentDamage = damageGolpe2;
                animator.SetTrigger("Attack2");
                StartCoroutine(AttackWindow(0.25f, 0.15f));
            }
            else
            {
                currentDamage = damageGolpe1;
                animator.SetTrigger("Attack1");
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

    public int GetCurrentDamage()
    {
        return currentDamage;
    }
}
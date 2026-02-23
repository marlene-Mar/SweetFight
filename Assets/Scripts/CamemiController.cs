using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CamemiController : MonoBehaviour
{
    public enum CamemiState
    {
        Patrolling,
        Greeting,
        Talking,
        Combat
    }

    [Header("Referencias")]
    private NavMeshAgent agent;
    private Animator animator;

    public Transform player;
    public DialogueManager dialogueManager;
    public Dialogos dialogoEncuentro;

    [Header("Combate")]
    public CombatManager combatManager;
    public CamemiHitbox[] hitboxes;
    private int comboCounter = 0;
    private float attackCooldown = 1.2f;
    private float attackTimer;
    private bool isBlocking = false;

    public int vidaMax = 100;
    private int vidaActual;

    private PompompurinController playerController;

    [Header("Patrulla")]
    public float patrolRadius = 10f;
    public float waitTimeBetweenPoints = 2f;

    [Header("Detección")]
    public float detectionRange = 5f;

    private float waitTimer;
    private CamemiState currentState;
    private bool canInteract = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        playerController = FindObjectOfType<PompompurinController>();

        currentState = CamemiState.Patrolling;

        vidaActual = vidaMax;
        agent.isStopped = false;
        agent.ResetPath();

        MoveToRandomPoint();
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case CamemiState.Patrolling:
                PatrolBehaviour();
                DetectPlayer();
                break;

            case CamemiState.Greeting:
            case CamemiState.Talking:
                LookAtPlayer();
                break;

            case CamemiState.Combat:
                LookAtPlayer();
                CombatBehaviour();
                break;
        }
    }

    // ========================
    // PATRULLA
    // ========================

    void PatrolBehaviour()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            animator.SetBool("Walk", false);
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeBetweenPoints)
            {
                MoveToRandomPoint();
                waitTimer = 0f;
            }
        }
        else
        {
            animator.SetBool("Walk", true);
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // ========================
    // DETECCIÓN
    // ========================

    void DetectPlayer()
    {
        if (!canInteract) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            canInteract = false;
            StartDialogue();
        }
    }

    // ========================
    // DIÁLOGO
    // ========================

    void StartDialogue()
    {
        agent.isStopped = true;
        animator.SetBool("Walk", false);
        animator.SetBool("InDialogue", true);
        currentState = CamemiState.Talking;

        if (dialogoEncuentro != null && dialogueManager != null)
        {
            dialogueManager.StartCamemiDialogue(dialogoEncuentro, this);
        }
    }

    public void EndDialogue()
    {
        currentState = CamemiState.Combat;

        animator.SetBool("InDialogue", false);

        playerController?.ExitDialogue();
        playerController?.StartCombatAfterDialogue();

        combatManager?.StartCamemiCombat(this, playerController);

        //if (manoCollider != null)
        //    manoCollider.SetActive(true);
        agent.isStopped = false;
        StartCombat();
    }

    // ========================
    // COMBATE
    // ========================

    void StartCombat()
    {
        currentState = CamemiState.Combat;
        animator.SetBool("Combat", true);
        agent.isStopped = false;
        animator.SetLayerWeight(1, 1f);
    }

    void CombatBehaviour()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        bool isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Cross Punch") ||
                            animator.GetCurrentAnimatorStateInfo(0).IsName("Boxing") ||
                            animator.GetCurrentAnimatorStateInfo(0).IsName("Body Block") ||
                            animator.GetCurrentAnimatorStateInfo(0).IsName("Kidney Hit");

        if (!isAttacking)
        {
            if (distance > 2f)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetBool("Walk", true);
            }
            else
            {
                agent.isStopped = true;
                animator.SetBool("Walk", false);

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    ExecuteCombo();
                    attackTimer = 0f;
                }
            }
        }
    }

    void ExecuteCombo()
    {
        comboCounter++;

        if (comboCounter <= 2)
        {
            animator.SetTrigger("Attack1");
            Debug.Log("Camemi usa Golpe1");
        }
        else
        {
            animator.SetTrigger("Attack2");
            Debug.Log("Camemi usa Golpe2");
            comboCounter = 0;
        }
    }

    public void TryBlock()
    {
        isBlocking = Random.value > 0.7f; // 30% probabilidad

        if (isBlocking)
            animator.SetTrigger("Block");
    }

    public void TakeDamage(int damage)
    {
        TryBlock();

        if (isBlocking)
        {
            Debug.Log("Camemi bloqueó el golpe!");
            return;
        }

        vidaActual -= damage;

        Debug.Log("Camemi recibe daño: " + damage +
                  " | Vida restante: " + vidaActual);

        animator.SetTrigger("RecibirGolpe");

        if (vidaActual <= 0)
        {
            Die();
        }
    }

    public void EnableHitboxes()
    {
        foreach (var hitbox in hitboxes)
        {
            hitbox.gameObject.SetActive(true);
            hitbox.ResetHit();
        }
    }

    public void DisableHitboxes()
    {
        foreach (var hitbox in hitboxes)
        {
            hitbox.gameObject.SetActive(false);
        }
    }

    // ========================
    // MIRAR AL JUGADOR
    // ========================

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    // ========================
    // VOLVER A PATRULLAR
    // ========================

    public void ReturnToPatrol()
    {
        currentState = CamemiState.Patrolling;

        agent.isStopped = false;
        agent.ResetPath();

        MoveToRandomPoint();
        canInteract = true;
    }

    // ========================
    // MUERTE
    // ========================
    void Die()
    {
        Debug.Log("Camemi ha sido derrotada");
        animator.SetLayerWeight(1, 0f);
        animator.SetTrigger("Morir");

        agent.isStopped = true;

        CombatManager.Instance.EndCombat(true);

        StartCoroutine(FinalSequence());
    }

    IEnumerator FinalSequence()
    {
        yield return new WaitForSeconds(3f);

        Debug.Log("Inicia diálogo final");

        // aquí llamas tu diálogo final

        yield return new WaitForSeconds(5f);

        //UIManager.Instance.ShowFinalScreen();
    }
}
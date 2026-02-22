using UnityEngine;
using UnityEngine.AI;

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
    public Collider[] manoCollider;

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

        StartCombat();
    }

    // ========================
    // COMBATE
    // ========================

    void StartCombat()
    {
        currentState = CamemiState.Combat;
        agent.isStopped = false;
    }

    void CombatBehaviour()
    {
        float distance = Vector3.Distance(transform.position, player.position);

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
            // animator.SetTrigger("Attack");
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
}
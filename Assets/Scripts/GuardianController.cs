using UnityEngine;
using UnityEngine.AI;

public class GuardianController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private MeshCollider[] validSurfaces;
    private Transform player;

    private DialogueManager dialogueManager;

    public float patrolRadius = 25f;
    public float waitTimeBetweenPoints = 1.5f;

    public float detectionDistance = 5f;

    private float waitTimer;
    private bool hasDestination;

    private enum GuardianState
    {
        Patrolling,
        Greeting,
        Talking,
        Combat
    }

    private GuardianState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
    {
        validSurfaces = surfaces;
        player = playerTransform;

        currentState = GuardianState.Patrolling;
        SetWalk(true);
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case GuardianState.Patrolling:
                PatrolBehaviour();
                DetectPlayer();
                break;

            case GuardianState.Greeting:
            case GuardianState.Talking:
            case GuardianState.Combat:
                break;
        }
    }

    void PatrolBehaviour()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetWalk(false);

            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeBetweenPoints)
            {
                MoveToRandomPoint();
                waitTimer = 0;
            }
        }
        else
        {
            SetWalk(true);
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            hasDestination = true;
        }
    }

    void DetectPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionDistance)
        {
            StartGreeting();
        }
    }

    void StartGreeting()
    {
        currentState = GuardianState.Greeting;

        agent.isStopped = true;

        SetWalk(false);

        transform.LookAt(player);

        animator.SetBool("isGreeting", true);

        Invoke(nameof(StartDialogue), 2f); 
    }

    void StartDialogue()
    {
        animator.SetBool("isGreeting", false);

        currentState = GuardianState.Talking;

        if (dialogueManager != null)
        {
            dialogueManager.StartGuardianDialogue(this);
        }
    }

    public void EndDialogue()
    {
        currentState = GuardianState.Combat;

        StartCombat();
    }

    void StartCombat()
    {
        animator.SetTrigger("GolpeP");
    }

    void SetWalk(bool value)
    {
        animator.SetBool("Walk", value);
    }
}
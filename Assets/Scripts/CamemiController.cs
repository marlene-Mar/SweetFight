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
            canInteract = false; // BLOQUEA INMEDIATAMENTE
            //StartCoroutine(DialogueSequence());
        }
    }

    // ========================
    // DIÁLOGO
    // ========================

    //IEnumerator DialogueSequence()
    //{
    //    Debug.Log("Iniciando diálogo Camemi");

    //    currentState = CamemiState.Greeting;

    //    agent.isStopped = true;
    //    agent.ResetPath();
    //    animator.SetBool("Walk", false);

    //    if (dialogueManager == null)
    //    {
    //        Debug.LogError("DialogueManager NO asignado");
    //        yield break;
    //    }

    //    if (dialogoEncuentro == null)
    //    {
    //        Debug.LogError("DialogoEncuentro NO asignado");
    //        yield break;
    //    }

    //    dialogueManager.GetConversation(dialogoEncuentro);

    //    currentState = CamemiState.Talking;

    //    yield return new WaitUntil(() => !dialogueManager.IsDialogue2Active());

    //    StartCombat();
    //}

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

    public void EndDialogue() { }

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
            agent.SetDestination(player.position);
            animator.SetBool("Walk", true);
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("Walk", false);
            // animator.SetTrigger("Attack"); si quieres
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
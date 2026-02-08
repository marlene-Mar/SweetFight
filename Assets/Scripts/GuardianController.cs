using UnityEngine;
using UnityEngine.AI;

public class GuardianController : MonoBehaviour
{
    public Transform player;
    public float minPatrolDistance = 15f;
    public float maxPatrolDistance = 40f;
    public float patrolSearchRadius = 60f;
    public float distanceToGreet = 5f;
    public float greetDuration = 3f;

    private NavMeshAgent navAgent;
    private Animator animator;

    private bool isInitialized = false;
    private bool isGreeting = false;

    private enum GuardianState
    {
        Patrolling,
        Greeting
    }

    private GuardianState currentState;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = 2.0f;
        animator = GetComponent<Animator>();

        navAgent.enabled = false;
    }

    void Update()
    {
        if (!isInitialized)
        {
            TryInitialize();
            return;
        }

        switch (currentState)
        {
            case GuardianState.Patrolling:
                CheckPlayerDistance();
                CheckIfReachedDestination();
                break;

            case GuardianState.Greeting:
                LookAtPlayer();
                break;
        }

        UpdateAnimations();
    }

    public void Initialize(MeshCollider[] surfaces, Transform playerTransform)
    {
        player = playerTransform;
    }

    void TryInitialize()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            navAgent.enabled = true;

            isInitialized = true;
            StartPatrolling();

            Debug.Log($"✓ {gameObject.name} listo para recorrer el mapa completo");
        }
    }

    void StartPatrolling()
    {
        currentState = GuardianState.Patrolling;
        GoToRandomNavMeshPoint();
    }

    void GoToRandomNavMeshPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolSearchRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolSearchRadius, NavMesh.AllAreas))
            {
                float distance = Vector3.Distance(transform.position, hit.position);

                if (distance >= minPatrolDistance && distance <= maxPatrolDistance)
                {
                    navAgent.isStopped = false;
                    navAgent.SetDestination(hit.position);
                    return;
                }
            }
        }
    }

    void CheckIfReachedDestination()
    {
        if (!navAgent.pathPending && navAgent.remainingDistance < 1.5f)
        {
            GoToRandomNavMeshPoint();
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null || isGreeting) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= distanceToGreet)
        {
            StartGreeting();
        }
    }

    void StartGreeting()
    {
        isGreeting = true;
        currentState = GuardianState.Greeting;

        navAgent.isStopped = true;
        navAgent.ResetPath();

        if (animator != null)
            animator.SetTrigger("Greet");

        Invoke(nameof(ReturnToPatrol), greetDuration);
    }

    void ReturnToPatrol()
    {
        isGreeting = false;
        currentState = GuardianState.Patrolling;
        GoToRandomNavMeshPoint();
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    void UpdateAnimations()
    {
        if (animator == null || !navAgent.enabled) return;

        float speed = navAgent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("Walk", speed > 0.1f);
    }
}
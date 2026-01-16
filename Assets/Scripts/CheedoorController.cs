using UnityEngine;
using UnityEngine.AI;

public class CheedoorController : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject player;
    private Animator agentAnimator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        agentAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        agent.SetDestination(player.transform.position);
        agentAnimator.SetFloat("Speed", agent.velocity.sqrMagnitude);
    }
}

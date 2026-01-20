using UnityEngine;
using UnityEngine.AI;
using System;

public class CheedoorController : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject player;
    private Animator agentAnimator;

    public Action OnDeath;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        agentAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        agent.SetDestination(player.transform.position);

        float speed = agent.velocity.magnitude;
        agentAnimator.SetFloat("Speed", speed);
    }

    public void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
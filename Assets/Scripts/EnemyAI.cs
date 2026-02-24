using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public int damage = 10;

    private Transform player;
    private NavMeshAgent agent;
    private Estado estadoActual;

    public enum Estado { Idle, Perseguir, Atacar }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        estadoActual = Estado.Idle;
    }

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) return;

        // Forzar al agente a anclarse en la posición actual del objeto
        agent.enabled = false;
        agent.enabled = true;

        // Asegurarse de warpear a la posición correcta
        agent.Warp(transform.position);

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        estadoActual = Estado.Idle;
    }

    void Update()
    {
        if (player == null || agent == null || !agent.isOnNavMesh) return;

        float distancia = Vector3.Distance(transform.position, player.position);

        switch (estadoActual)
        {
            case Estado.Idle:
                agent.isStopped = true;
                if (distancia <= detectionRange)
                    estadoActual = Estado.Perseguir;
                break;

            case Estado.Perseguir:
                agent.isStopped = false;
                agent.SetDestination(player.position);
                if (distancia <= attackRange)
                    estadoActual = Estado.Atacar;
                if (distancia > detectionRange)
                    estadoActual = Estado.Idle;
                break;

            case Estado.Atacar:
                agent.isStopped = true;
                if (distancia > attackRange)
                    estadoActual = Estado.Perseguir;
                break;
        }
    }
}
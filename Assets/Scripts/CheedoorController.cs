using UnityEngine;
using UnityEngine.AI;
using System;

public class CheedoorController : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject player;
    private Animator agentAnimator;

    //Combate
    public int damage;
    public int maxHealth = 50;
    public bool isAttacking;

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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Mano")) return;

        PompompurinController player = other.GetComponentInParent<PompompurinController>();

        if (player != null && player.IsAttacking())
        {
            int dmg = player.GetCurrentDamage();

            GameManager hp = GetComponent<GameManager>();
            if (hp != null)
                hp.TakeDamage(dmg);
        }
    }


    public void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

}
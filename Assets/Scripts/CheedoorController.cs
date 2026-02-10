using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class CheedoorController : MonoBehaviour
{
    [Header("Referencias")]
    private NavMeshAgent agent;
    private GameObject player;
    private Animator agentAnimator;

    [Header("Combate")]
    public int damage = 10;
    public int maxHealth = 50;
    private int currentHealth;
    public bool isAttacking;
    public Action OnDeath;

    [Header("Configuración de Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;
    private bool isKnockedBack = false;

    [Header("Configuración de Movimiento")]
    public float stoppingDistance = 1.5f;

    [Header("Configuración de Daño al Jugador")]
    public float damageCooldown = 1f;
    private float lastDamageTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        agentAnimator = GetComponent<Animator>();

        currentHealth = maxHealth;
        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (player == null || isKnockedBack) return;

        // Perseguir al jugador
        agent.SetDestination(player.transform.position);

        // Actualizar animación de velocidad
        float speed = agent.velocity.magnitude;
        agentAnimator.SetFloat("Speed", speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar golpe del jugador
        //if (other.CompareTag("Mano"))
        //{
        //    PlayerCombat playerController = other.GetComponentInParent<PlayerCombat>();
        //    if (playerController != null && playerController.IsAttacking())
        //    {
        //        int dmg = playerController.GetCurrentDamage();

        //        // Calcular dirección del empuje (desde las manos hacia el enemigo)
        //        Vector3 knockbackDirection = (transform.position - other.transform.position).normalized;

        //        TakeDamage(dmg, knockbackDirection);
        //    }
        //}
    }

    private void OnTriggerStay(Collider other)
    {
        // Hacer daño al jugador cuando lo toca
        if (other.CompareTag("Player") && Time.time - lastDamageTime >= damageCooldown)
        {
            PompompurinController playerController = other.GetComponent<PompompurinController>();
            if (playerController != null)
            {
                // playerController.TakeDamage(damage);
                Debug.Log($"Cheedoor toca al jugador y causa {damage} de daño");
                lastDamageTime = Time.time;
            }
        }
    }

    public void TakeDamage(int damage, Vector3 knockbackDirection)
    {
        currentHealth -= damage;
        Debug.Log($"Cheedoor recibió {damage} de daño. Salud: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Aplicar empuje
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }
    }

    private IEnumerator ApplyKnockback(Vector3 direction)
    {
        isKnockedBack = true;
        agent.isStopped = true;

        // Calcular posición de destino del empuje
        Vector3 knockbackTarget = transform.position + (direction * knockbackForce);

        // Verificar que la posición sea válida en el NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(knockbackTarget, out hit, knockbackForce, NavMesh.AllAreas))
        {
            knockbackTarget = hit.position;
        }

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        // Mover hacia atrás suavemente
        while (elapsedTime < knockbackDuration)
        {
            transform.position = Vector3.Lerp(startPosition, knockbackTarget, elapsedTime / knockbackDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = knockbackTarget;

        // Reactivar el NavMeshAgent
        agent.isStopped = false;
        isKnockedBack = false;
    }

    public void Die()
    {
        agent.isStopped = true;
        enabled = false;

        OnDeath?.Invoke();

        Destroy(gameObject, 0.5f);
    }
}
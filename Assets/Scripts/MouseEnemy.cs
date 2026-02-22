using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class MouseEnemy : MonoBehaviour
{
    [Header("Referencias")]
    private NavMeshAgent agent;
    private GameObject player;
    private Animator agentAnimator;

    [Header("Combate")]
    public int damage = 10;
    public int maxHealth = 25;
    private int currentHealth;
    private bool isDead = false;
    public Action OnDeath;

    [Header("Configuración de Knockback")]
    public float knockbackForce = 4f;
    public float knockbackDuration = 0.25f;
    private bool isKnockedBack = false;

    [Header("Configuración de Movimiento")]
    public float stoppingDistance = 1.2f;

    [Header("Configuración de Daño al Jugador")]
    public float damageCooldown = 1.2f;
    private float lastDamageTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        agentAnimator = GetComponent<Animator>();

        currentHealth = maxHealth;

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            // Forzar al agente a tocar el NavMesh al aparecer
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
    }

    void Update()
    {
        if (player == null || isKnockedBack || isDead) return;

        // Perseguir al jugador si el agente está activo
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(player.transform.position);
        }

        // Actualizar animación (Asegúrate de tener el parámetro "Speed" en el Animator)
        if (agentAnimator != null)
        {
            float speed = agent.velocity.magnitude;
            agentAnimator.SetFloat("Speed", speed);
        }
    }

    // Esta es la función que te pedía el script del jugador
    public bool CanReceiveDamage()
    {
        return !isDead && !isKnockedBack;
    }

    public void TakeDamage(int damageReceived, Vector3 knockbackDirection)
    {
        if (isDead) return;

        currentHealth -= damageReceived;
        Debug.Log($"Ratón recibió daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }
    }

    private IEnumerator ApplyKnockback(Vector3 direction)
    {
        isKnockedBack = true;

        if (agent.isActiveAndEnabled)
            agent.isStopped = true;

        // Calculamos la posición de destino del empuje
        Vector3 knockbackTarget = transform.position + (direction * knockbackForce);

        // Verificamos que el punto de destino sea válido en el NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(knockbackTarget, out hit, knockbackForce, NavMesh.AllAreas))
        {
            knockbackTarget = hit.position;
        }

        yield return new WaitForSeconds(knockbackDuration);

        // Mover al agente físicamente a la nueva posición tras el golpe
        if (!isDead)
        {
            agent.Warp(knockbackTarget);
            agent.isStopped = false;
        }

        isKnockedBack = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead) return;

        // Hacer daño al jugador si entra en el trigger del ratón
        if (other.CompareTag("Player") && Time.time - lastDamageTime >= damageCooldown)
        {
            // Aquí busca el script de tu jugador para restarle vida
            Debug.Log("El Ratón está mordiendo al jugador!");
            lastDamageTime = Time.time;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (agent != null)
            agent.enabled = false;

        OnDeath?.Invoke();

        // Si tienes animación de muerte, actívala aquí
        // agentAnimator.SetTrigger("Die");

        Destroy(gameObject, 0.2f);
    }
}
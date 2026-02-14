using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Daño por cada tick (cada intervalo)")]
    public float damagePerTick = 8f;

    [Tooltip("Intervalo entre aplicaciones de daño (segundos)")]
    public float damageInterval = 0.8f;

    [Header("Opcional - Daño inicial al entrar")]
    [Tooltip("Daño extra al primer contacto (0 = desactivado)")]
    public float initialContactDamage = 12f;

    [Header("Comportamiento")]
    [Tooltip("¿Debe ignorar daño si el jugador está en diálogo o invencible?")]
    public bool respectDialogueOrInvincibility = true;

    private float nextDamageTime;

    // Referencia al jugador (cacheada la primera vez que lo toca)
    private VidaJugador playerHealth;


    void Start()
    {
        nextDamageTime = Time.time + damageInterval;
    }


    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Cacheamos la referencia la primera vez
        if (playerHealth == null)
        {
            playerHealth = other.GetComponent<VidaJugador>();
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<VidaJugador>();
            }

            if (playerHealth == null)
            {
                Debug.LogWarning("No se encontró VidaJugador en el Player", other.gameObject);
                return;
            }
        }

        // Daño inicial (opcional)
        if (initialContactDamage > 0f)
        {
            // Puedes agregar aquí chequeo de invencibilidad si lo necesitas
            if (!respectDialogueOrInvincibility)
            {
                playerHealth.RecibirDaño(initialContactDamage);
                Debug.Log($"[{gameObject.name}] Daño inicial de contacto: {initialContactDamage}");
            }
        }

        // Preparamos el primer tick de daño continuo
        nextDamageTime = Time.time + damageInterval;
    }


    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerHealth == null) return;

        if (Time.time >= nextDamageTime)
        {
            if (!respectDialogueOrInvincibility)
            {
                playerHealth.RecibirDaño(damagePerTick);
                Debug.Log($"[{gameObject.name}] Daño por contacto: {damagePerTick} | Vida actual: {playerHealth.vidaActual}");

                // Animación o feedback visual/sonoro aquí si quieres
                // Ejemplo: playerHealth.GetComponent<Animator>().SetTrigger("RecibirGolpe");
            }

            nextDamageTime = Time.time + damageInterval;
        }
    }


    // Opcional: evita daño durante diálogo, invencibilidad, etc.
    //private bool IsPlayerProtected()
    //{
    //    // Ejemplo 1: usando PompompurinController
    //    var controller = playerHealth.GetComponent<PompompurinController>();
    //    if (controller != null && controller.InDialogue)
    //        return true;

    //    // Ejemplo 2: si tienes un sistema de invencibilidad (i-frames)
    //    // if (playerHealth.IsInvincible()) return true;

    //    return false;
    //}


    // Opcional: para debugging visual
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, GetComponent<Collider>()?.bounds.extents.magnitude ?? 1.5f);
    }
}
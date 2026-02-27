using UnityEngine;

public class CamemiHitbox : MonoBehaviour
{
    public int damage;
    private bool hasHit;
    private Collider hitCollider;

    private void Awake()
    {
        hitCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        // Resetear siempre al activarse para garantizar detección en combos rápidos
        hasHit = false;
        Debug.Log($"[Hitbox] {gameObject.name} ACTIVADA");
    }

    private void OnDisable()
    {
        // Reset también al desactivar, por si se reactiva muy rápido en un combo
        hasHit = false;
        Debug.Log($"[Hitbox] {gameObject.name} DESACTIVADA");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (!other.CompareTag("Player")) return;

        hasHit = true;
        Debug.Log($"[Hitbox] GOLPE CONFIRMADO - {gameObject.name} → daño: {damage}");
        CombatManager.Instance?.OnCamemiHit(damage);
    }

    // Respaldo para cuando el jugador ya está DENTRO del collider al activarse
    // Ocurre frecuentemente en combos rápidos y cuando el personaje se mueve
    private void OnTriggerStay(Collider other)
    {
        if (hasHit) return;
        if (!other.CompareTag("Player")) return;

        hasHit = true;
        Debug.Log($"[Hitbox] GOLPE (Stay) - {gameObject.name} → daño: {damage}");
        CombatManager.Instance?.OnCamemiHit(damage);
    }
}
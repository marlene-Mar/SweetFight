using UnityEngine;

public class CamemiHitbox : MonoBehaviour
{
    public int damage;
    private bool hasHit;

    private void OnEnable()
    {
        hasHit = false;
        Debug.Log($"[Hitbox] {gameObject.name} ACTIVADA - hasHit reseteado");
    }

    private void OnDisable()
    {
        Debug.Log($"[Hitbox] {gameObject.name} DESACTIVADA");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Hitbox] {gameObject.name} contacto con: {other.gameObject.name} | Tag: {other.tag}");

        if (hasHit) return;
        if (!other.CompareTag("Player")) return;

        hasHit = true;
        Debug.Log($"[Hitbox] GOLPE CONFIRMADO - {gameObject.name} → daño: {damage}");

        // Desactiva todas las hitboxes del padre para evitar doble golpe
        //foreach (var hb in transform.parent.GetComponentsInChildren<CamemiHitbox>())
        //    hb.gameObject.SetActive(false);

        CombatManager.Instance?.OnCamemiHit(damage);
    }
}
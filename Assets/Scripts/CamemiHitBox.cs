using UnityEngine;

public class CamemiHitbox : MonoBehaviour
{
    public int damage = 10;
    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;

            Debug.Log("Camemi golpeó al jugador (Hitbox)");

            CombatManager.Instance?.OnCamemiHit(damage);
        }
    }

    public void ResetHit()
    {
        hasHit = false;
    }
}
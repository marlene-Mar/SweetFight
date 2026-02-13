using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public int damage = 15; // Daño de la lanza
    private CombatManager combatManager;

    void Start()
    {
        combatManager = FindObjectOfType<CombatManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Verificar si golpeó a Pompompurin
        if (other.CompareTag("Player") || other.name.Contains("Pompompurin"))
        {
            PompompurinController player = other.GetComponent<PompompurinController>();

            if (player == null)
                player = other.GetComponentInParent<PompompurinController>();

            if (player != null)
            {
                Debug.Log($"¡Lanza golpeó a Pompompurin! Daño: {damage}");

                // Notificar al CombatManager
                if (combatManager != null)
                {
                    combatManager.OnGuardianHit(damage);
                }
            }
        }
    }
}
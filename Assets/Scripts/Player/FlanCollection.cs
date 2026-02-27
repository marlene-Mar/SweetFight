using UnityEngine;

// Clase que se encarga de detectar la colision con los flanes y sumar flanes al jugador
public class FlanCollection : MonoBehaviour
{
    private int flanCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Flan"))
        {
            flanCount++;

            ItemPickup pickup = other.GetComponent<ItemPickup>();

            if (pickup != null)
            {
                InventoryManager.instance.AddItem(pickup.itemData, 1);
            }

            Destroy(other.gameObject);

            Debug.Log($"Flanes recogidos: {flanCount}");
        }
    }
}
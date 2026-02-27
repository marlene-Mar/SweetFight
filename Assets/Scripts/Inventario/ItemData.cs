using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/New Item")]

// Clase para definir los datos de un item: actualmente solo flanes
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Consumable,
        Equipment
    }

    public string itemName;
    public Sprite itemIcon;
    public string itemDescription;
    public ItemType itemType;
    
}

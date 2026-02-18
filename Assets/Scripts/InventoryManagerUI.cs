using UnityEngine;
using System.Collections.Generic;

public class InventoryManagerUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform inventoryContainer;

    public void RefreshInventoryUI(List<Item> inventory)
    {
        foreach (Transform t in inventoryContainer)
        {
            Destroy(t.gameObject);
        }

        foreach (Item item in inventory)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, inventoryContainer);
            ItemSlotUI slotUI = newSlot.GetComponent<ItemSlotUI>();

            slotUI.itemIconImage.sprite = item.itemData.itemIcon;
            slotUI.itemQuantityText.text = "x" + item.itemQuantity.ToString();
        }
    }
}

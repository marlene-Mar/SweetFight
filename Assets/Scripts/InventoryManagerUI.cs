using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManagerUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform inventoryContainer;


    public void RefreshInventoryUI()
    {

        foreach (Transform t in inventoryContainer)
        {

            Destroy(t.gameObject);

        }

       
        foreach (Item item in InventoryManager.instance.inventory)
        {
            
                       GameObject newItemSlot = Instantiate(itemSlotPrefab, inventoryContainer);
            
                       ItemSlotUI itemSlotUI = newItemSlot.GetComponent<ItemSlotUI>();

            itemSlotUI.itemIconImage.sprite = item.itemData.itemIcon;
            itemSlotUI.itemNameText.text = item.itemData.itemName;
            itemSlotUI.itemQuantityText.text = "x" + item.itemQuantity.ToString();


        }



    }

    }

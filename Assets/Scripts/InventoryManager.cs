using UnityEngine;
using System.Collections.Generic;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public List<Item> inventory = new List<Item>();

    [Header("Debug")]
    public ItemData testItemData;
    public ItemData testItemData2;
    public ItemData testItemData3;
    public ItemData testItemData4;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }


    private void Start()
    {
        AddItem(testItemData, 1); 
        AddItem(testItemData2, 1);
        AddItem(testItemData, 2); 
        AddItem(testItemData3, 1);
        AddItem(testItemData4, 1);

        GetComponent<InventoryManagerUI>().RefreshInventoryUI();
    }


   public void AddItem(ItemData a, int b) //Suma la cantidad del mismo item, si no existe lo agrega a la lista
    {
    foreach (Item item in inventory)
        {
            if (item.itemData.itemName == a.itemName)
            {

                item.itemQuantity += b;
                return;
            }


        }    


     inventory.Add(new Item { itemData = a, itemQuantity = b });
   }


}

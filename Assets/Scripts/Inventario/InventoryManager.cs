//using UnityEngine;
//using System.Collections.Generic;


//public class InventoryManager : MonoBehaviour
//{
//    public static InventoryManager instance;

//    public List<Item> inventory = new List<Item>();

//    [Header("Debug")]
//    public ItemData testItemData;
//    public ItemData testItemData2;
//    public ItemData testItemData3;
//    public ItemData testItemData4;

//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }

//    }


//    private void Start()
//    {
//        AddItem(testItemData, 1); 
//        AddItem(testItemData2, 1);
//        AddItem(testItemData, 2); 
//        AddItem(testItemData3, 1);
//        AddItem(testItemData4, 1);

//        GetComponent<InventoryManagerUI>().RefreshInventoryUI();
//    }


//   public void AddItem(ItemData a, int b) //Suma la cantidad del mismo item, si no existe lo agrega a la lista
//    {
//    foreach (Item item in inventory)
//        {
//            if (item.itemData.itemName == a.itemName)
//            {

//                item.itemQuantity += b;
//                return;
//            }


//        }    


//     inventory.Add(new Item { itemData = a, itemQuantity = b });
//   }


//}

using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI")]
    public GameObject inventoryPanel;
    public InventoryManagerUI inventoryUI;

    [Header("Flan Config")]
    public ItemData flanData;

    private List<Item> inventory = new List<Item>();
    private VidaJugador vidaJugador;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        inventoryPanel.SetActive(false);
    }

    private void Start()
    {
        vidaJugador = FindObjectOfType<VidaJugador>();
    }

    private void Update()
    { 
        // Consumir flan
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseFlan();
        }
    }
    // =============================
    // BOTÓN INVENTARIO
    // =============================
    public void ToggleInventory()
    {
        if (inventory.Count == 0)
            return; // No abrir si está vacío

        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }


    // =============================
    // AGREGAR ITEM
    // =============================
    public void AddItem(ItemData itemData, int quantity)
    {
        foreach (Item item in inventory)
        {
            if (item.itemData == itemData)
            {
                item.itemQuantity += quantity;
                inventoryUI.RefreshInventoryUI(inventory);
                inventoryPanel.SetActive(true); // Mostrar cuando obtenga algo
                return;
            }
        }

        inventory.Add(new Item { itemData = itemData, itemQuantity = quantity });

        inventoryUI.RefreshInventoryUI(inventory);
        inventoryPanel.SetActive(true);
    }

    // =============================
    // USAR FLAN
    // =============================
    private void UseFlan()
    {
        foreach (Item item in inventory)
        {
            if (item.itemData == flanData && item.itemQuantity > 0)
            {
                item.itemQuantity--;

                vidaJugador.Curar(5);

                if (item.itemQuantity <= 0)
                {
                    inventory.Remove(item);
                }

                inventoryUI.RefreshInventoryUI(inventory);
                return;
            }
        }
    }
}

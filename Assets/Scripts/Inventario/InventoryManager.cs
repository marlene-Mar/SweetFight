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

    public void ResetInventory()
    {
        inventory.Clear();
        inventoryUI.RefreshInventoryUI(inventory);
        inventoryPanel.SetActive(false);
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

    [Header("Save Data")]
    public Data gameData; // asígnalo en el Inspector

    // Llama esto antes de guardar la partida
    public void SaveToData()
    {
        foreach (Item item in inventory)
        {
            if (item.itemData == flanData)
            {
                gameData.flanCount = item.itemQuantity;
                return;
            }
        }
        gameData.flanCount = 0; // si no hay flanes en inventario
    }

    // Llama esto al cargar la partida
    public void LoadFromData()
    {
        if (gameData.flanCount > 0)
            AddItem(flanData, gameData.flanCount);
    }
}

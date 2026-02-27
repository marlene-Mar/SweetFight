using UnityEngine;
using System.Collections.Generic;

// Clase para manejar el inventario del jugador: actualmente solo flanes
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI")]
    public GameObject inventoryPanel;
    public InventoryManagerUI inventoryUI;

    [Header("Flan Config")]
    public ItemData flanData; 

    private List<Item> inventory = new List<Item>(); // Lista de items en el inventario
    private VidaJugador vidaJugador; // Referencia a la vida del jugador para curar al usar flan

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
        // Consumir flan con la tecla F
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseFlan();
        }
    }

    // Resetea el inventario cuando muere el jugador o se reinicie la partida
    public void ResetInventory()
    {
        inventory.Clear();
        inventoryUI.RefreshInventoryUI(inventory);
        inventoryPanel.SetActive(false);
    }

    // =====================================================
    // BOTÓN INVENTARIO: abrir/cerrar la barra de inventario
    // =====================================================
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
                item.itemQuantity += quantity; // Si ya existe el item, solo aumenta la cantidad
                inventoryUI.RefreshInventoryUI(inventory); // Actualiza la UI
                inventoryPanel.SetActive(true); // Mostrar cuando obtenga algo
                return;
            }
        }

        inventory.Add(new Item { itemData = itemData, itemQuantity = quantity }); // Si no existe el item, lo agrega como nuevo

        inventoryUI.RefreshInventoryUI(inventory); // Actualiza la UI
        inventoryPanel.SetActive(true); // Mostrar el panel de inventario cuando se agrega un nuevo item
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
                item.itemQuantity--; // Disminuye la cantidad de flanes al usar uno

                vidaJugador.Curar(5);

                if (item.itemQuantity <= 0)
                {
                    inventory.Remove(item); // Si no quedan flanes, lo elimina del inventario
                }

                inventoryUI.RefreshInventoryUI(inventory); // Actualiza la UI después de usar el flan
                return;
            }
        }
    }

    [Header("Save Data")]
    public Data gameData; 

    //Guarda la cantidad de flanes en el inventario al guardar la partida
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

    // Carga la cantidad de flanes en el inventario al cargar la partida
    public void LoadFromData()
    {
        if (gameData.flanCount > 0)
            AddItem(flanData, gameData.flanCount);
    }
}

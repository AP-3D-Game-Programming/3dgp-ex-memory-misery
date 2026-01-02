using UnityEngine;
using UnityEngine.SceneManagement; // Nodig voor de fix!
using System.Collections.Generic;

[System.Serializable]
public struct InventoryItem
{
    public ItemData itemData;
    public int quantity;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Settings")]
    private int maxSlots = 3; // Of hoeveel slots je ook hebt

    // De inventory data (blijft bestaan na reload)
    private List<InventoryItem> inventory = new List<InventoryItem>();

    public event System.Action OnInventoryChanged;

    [Header("UI References")]
    public GameObject inventoryUIPanel; // De container in de Canvas
    public GameObject slotPrefab;       // Het prefab knopje

    private InventorySlotUI[] uiSlots;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Blijft bestaan na scene wissel
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateSlots();
        OnInventoryChanged += UpdateInventoryUI;
    }

    // === DE FIX VOOR DE ERROR ===
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Deze functie runt elke keer als je herstart/doodgaat.
        // We zoeken het NIEUWE paneel via de Tag 'InventoryGrid'.
        GameObject foundPanel = GameObject.FindGameObjectWithTag("InventoryGrid");

        if (foundPanel != null)
        {
            inventoryUIPanel = foundPanel;
            GenerateSlots();      // Maak nieuwe slots
            UpdateInventoryUI();  // Vul ze met je huidige items
        }
    }
    // ============================

    private void GenerateSlots()
    {
        if (inventoryUIPanel == null) return;

        // Oude slots weggooien (voor de zekerheid)
        foreach (Transform child in inventoryUIPanel.transform)
        {
            Destroy(child.gameObject);
        }

        uiSlots = new InventorySlotUI[maxSlots];

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, inventoryUIPanel.transform);
            uiSlots[i] = slotObject.GetComponent<InventorySlotUI>();
        }
    }

    private void UpdateInventoryUI()
    {
        if (uiSlots == null) return;

        for (int i = 0; i < maxSlots; i++)
        {
            // Check of het slot nog bestaat (voorkomt jouw error)
            if (uiSlots[i] == null) continue;

            if (i < inventory.Count)
            {
                uiSlots[i].SetItem(inventory[i].itemData, inventory[i].quantity);
            }
            else
            {
                uiSlots[i].SetItem(null, 0);
            }
        }
    }

    public bool AddItem(ItemData itemToAdd, int amount = 1)
    {
        // 1. Stackable Logic
        if (itemToAdd.maxStackSize > 1)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].itemData == itemToAdd && inventory[i].quantity < itemToAdd.maxStackSize)
                {
                    int spaceLeft = itemToAdd.maxStackSize - inventory[i].quantity;
                    int amountToStack = Mathf.Min(amount, spaceLeft);

                    inventory[i] = new InventoryItem
                    {
                        itemData = itemToAdd,
                        quantity = inventory[i].quantity + amountToStack
                    };
                    amount -= amountToStack;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        // Roep hier notification aan als je die hebt:
                        if (NotificationManager.Instance != null) NotificationManager.Instance.ShowNotification(itemToAdd);
                        return true;
                    }
                }
            }
        }

        // 2. New Slot Logic
        while (amount > 0 && inventory.Count < maxSlots)
        {
            int amountToPlace = Mathf.Min(amount, itemToAdd.maxStackSize);
            inventory.Add(new InventoryItem { itemData = itemToAdd, quantity = amountToPlace });
            amount -= amountToPlace;

            // Roep hier notification aan als je die hebt:
            if (NotificationManager.Instance != null) NotificationManager.Instance.ShowNotification(itemToAdd);
        }

        OnInventoryChanged?.Invoke();
        return amount <= 0;
    }

    public bool RemoveItem(ItemData itemToRemove, int amount = 1)
    {
        int amountToRemove = amount;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].itemData == itemToRemove)
            {
                if (inventory[i].quantity > amountToRemove)
                {
                    inventory[i] = new InventoryItem
                    {
                        itemData = itemToRemove,
                        quantity = inventory[i].quantity - amountToRemove
                    };
                    amountToRemove = 0;
                    break;
                }
                else
                {
                    amountToRemove -= inventory[i].quantity;
                    inventory.RemoveAt(i);
                }
            }
        }

        if (amountToRemove == 0)
        {
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public List<InventoryItem> GetInventory()
    {
        return inventory;
    }

    // Nodig voor de Deur logic
    public bool HasItem(ItemData itemToCheck)
    {
        foreach (InventoryItem slot in inventory)
        {
            if (slot.itemData == itemToCheck && slot.quantity > 0)
            {
                return true;
            }
        }
        return false;
    }

    // Nodig voor Death Screen (resetten)
    public void ClearInventory()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke();
        Debug.Log("Inventory Cleared");
    }
}
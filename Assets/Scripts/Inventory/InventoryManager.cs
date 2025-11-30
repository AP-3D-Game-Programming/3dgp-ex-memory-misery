using UnityEngine;
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

    private int maxSlots = 3;

    private List<InventoryItem> inventory = new List<InventoryItem>();

    public event System.Action OnInventoryChanged;

    [Header("UI References")]
    public GameObject inventoryUIPanel;
    public GameObject slotPrefab;

    private InventorySlotUI[] uiSlots;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    private void Update()
    {
    }

    private void GenerateSlots()
    {
        uiSlots = new InventorySlotUI[maxSlots];

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, inventoryUIPanel.transform);
            uiSlots[i] = slotObject.GetComponent<InventorySlotUI>();
        }
    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < maxSlots; i++)
        {
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
                        return true;
                    }
                }
            }
        }

        while (amount > 0 && inventory.Count < maxSlots)
        {
            int amountToPlace = Mathf.Min(amount, itemToAdd.maxStackSize);

            inventory.Add(new InventoryItem { itemData = itemToAdd, quantity = amountToPlace });
            amount -= amountToPlace;
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
}
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item To Pick Up")]
    public ItemData item;
    public int amount = 1;

    public void PickUp()
    {
        if (InventoryManager.Instance.AddItem(item, amount))
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory full!");
        }
    }

    public void Highlight(bool state)
    {
        // Nog bekijken
    }
}
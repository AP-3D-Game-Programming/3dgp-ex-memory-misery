using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item To Pick Up")]
    public ItemData item;
    public int amount = 1;

    [Header("Interaction Settings")]
    public bool isTrigger = true;

    public void PickUp()
    {
        if (InventoryManager.Instance == null || item == null)
        {
            return;
        }

        bool addedSuccessfully = InventoryManager.Instance.AddItem(item, amount);

        if (addedSuccessfully)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"Inventory full, cannot pick up {item.itemName}.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTrigger)
        {
            if (other.CompareTag("Player"))
            {
                PickUp();
            }
        }
    }
}
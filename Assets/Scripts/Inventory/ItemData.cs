using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName = "New Item";
    public Sprite icon;
    
    [TextArea(3, 5)]
    public string description = "A description of the item.";

    [Header("Stacking & Type")]
    public int maxStackSize = 1; 
    public bool isKeyItem = false;
    
    public virtual void UseItem()
    {
        Debug.Log("Used item: " + itemName);
    }
}
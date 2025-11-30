using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    private ItemData currentItem;

    public void SetItem(ItemData data, int quantity)
    {
        currentItem = data;

        if (data != null)
        {
            itemIcon.sprite = data.icon;
            itemIcon.enabled = true;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
        else
        {
            itemIcon.enabled = false;
            quantityText.text = "";
        }
    }

    public void OnSlotClicked()
    {
        if (currentItem != null)
        {
            Debug.Log($"Item {currentItem.itemName} selected/used.");
            // Voeg hier de logica toe voor itemgebruik
        }
    }
}
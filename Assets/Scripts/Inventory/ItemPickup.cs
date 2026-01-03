using UnityEngine;
using UnityEngine.Events;

public class ItemPickup : MonoBehaviour
{
    [Header("Speciale Actie")]
    public UnityEvent onPickup;

    [Header("Item To Pick Up")]
    public ItemData item;
    public int amount = 1;

    private Renderer itemRenderer;
    private Color originalColor;

    // Voeg Start toe om de originele kleur te onthouden
    private void Start()
    {
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null)
        {
            originalColor = itemRenderer.material.color;
        }
    }

    public void PickUp()
    {
        // Jouw bestaande logica
        if (InventoryManager.Instance.AddItem(item, amount))
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory full!");
        }
        if (onPickup != null) onPickup.Invoke();

        Destroy(gameObject);
    }

    public void Highlight(bool state)
    {
        if (itemRenderer == null) return;

        if (state)
        {
            // Maak het item geel (of een emissive kleur) als je er naar kijkt
            itemRenderer.material.color = Color.yellow;
        }
        else
        {
            // Zet kleur terug naar normaal
            itemRenderer.material.color = originalColor;
        }
    }
}
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("UI")]
    public TMP_Text interactText;

    private Camera cam;
    private ItemPickup currentItem; // Hier onthouden we naar welk item we kijken

    private void Start()
    {
        cam = Camera.main;

        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        // Schiet de straal
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            ItemPickup newPickup = hit.collider.GetComponent<ItemPickup>();

            // Situatie 1: We kijken naar een item
            if (newPickup != null)
            {
                // Als we naar een NIEUW item kijken (of we keken eerst naar niks)
                if (currentItem != newPickup)
                {
                    // Zet de vorige uit (als die er was)
                    if (currentItem != null) currentItem.Highlight(false);

                    // Zet de nieuwe aan
                    currentItem = newPickup;
                    currentItem.Highlight(true);

                    // Toon tekst
                    if (interactText != null)
                    {
                        interactText.text = "Press E to pick up " + currentItem.item.itemName; // Tip: toon naam!
                        interactText.gameObject.SetActive(true);
                    }
                }

                // Check voor oppakken
                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentItem.PickUp();
                    ClearCurrentItem(); // Reset alles na oppakken
                }
                return; // Stop hier, we hebben een item gevonden
            }
        }

        // Situatie 2: We kijken nergens naar, of naar iets dat geen item is
        ClearCurrentItem();
    }

    private void ClearCurrentItem()
    {
        if (currentItem != null)
        {
            currentItem.Highlight(false);
            currentItem = null;
        }
        if (interactText != null) interactText.gameObject.SetActive(false);
    }
}
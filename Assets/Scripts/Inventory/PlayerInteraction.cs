using UnityEngine;
using TMPro; // Needed for TextMeshPro

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    public TMP_Text interactText; // Drag your UI text here in Inspector

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (interactText != null)
            interactText.gameObject.SetActive(false); // Hide text initially
    }

    private void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();

            if (pickup != null)
            {
                // Show the prompt
                if (interactText != null)
                {
                    interactText.text = "Press E to pick up";
                    interactText.gameObject.SetActive(true);
                }

                // Pick up item on key press
                if (Input.GetKeyDown(KeyCode.E))
                {
                    pickup.PickUp();
                }
            }
            else
            {
                // Hide if hit something not pickable
                if (interactText != null)
                    interactText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Hide when nothing is in range
            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }
}
